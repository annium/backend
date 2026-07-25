using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.MessageBus.Abstractions;
using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;

namespace Annium.MessageBus.Nats.Internal;

/// <summary>
/// A JetStream transport consumer bound to a single at-least-once (or replay) subscription. On start it resolves the
/// externally-provisioned stream capturing the subject, creates a pull consumer (a shared durable for a queue group so
/// members compete, or an ephemeral consumer for fan-out), and runs a background loop dispatching each message to the
/// pipeline callback. The received JetStream message is the acknowledgement token: <see cref="CompleteAsync"/> acks it
/// and <see cref="AbandonAsync"/> naks it for immediate redelivery. A start position other than "new" (replay) sets the
/// consumer's deliver policy accordingly.
/// </summary>
internal sealed class NatsJetStreamConsumer : ITransportConsumer, ILogSubject
{
    /// <summary>
    /// The logger for this consumer.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The shared connection (JetStream context).
    /// </summary>
    private readonly NatsConnectionHolder _connection;

    /// <summary>
    /// The subscription options.
    /// </summary>
    private readonly SubscriptionOptions _options;

    /// <summary>
    /// The start position; <see cref="StartPosition.New"/> for a plain (non-replay) subscription.
    /// </summary>
    private readonly StartPosition _startPosition;

    /// <summary>
    /// Cancellation source stopping the consume loop; its token is passed to the pipeline callback.
    /// </summary>
    private readonly CancellationTokenSource _stopCts = new();

    /// <summary>
    /// The pipeline callback invoked per delivery.
    /// </summary>
    private Func<TransportDelivery, CancellationToken, Task>? _onMessage;

    /// <summary>
    /// The background consume loop, started in <see cref="StartAsync"/> and awaited on dispose so the pull
    /// subscription is torn down before disposal returns.
    /// </summary>
    private Task _loop = Task.CompletedTask;

    /// <summary>
    /// Guards against repeated disposal and blocks ack/nack once stopping (a draining handler may still ack after the
    /// consumer is stopped).
    /// </summary>
    private volatile bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="NatsJetStreamConsumer"/> class.
    /// </summary>
    /// <param name="connection">The shared connection.</param>
    /// <param name="options">The subscription options.</param>
    /// <param name="logger">The logger.</param>
    public NatsJetStreamConsumer(NatsConnectionHolder connection, SubscriptionOptions options, ILogger logger)
    {
        _connection = connection;
        _options = options;
        // Plain subscriptions are not ReplaySubscriptionOptions — default to "New".
        _startPosition = (options as ReplaySubscriptionOptions)?.StartPosition ?? StartPosition.New;
        Logger = logger;
    }

    /// <summary>
    /// Resolves the externally-provisioned JetStream stream capturing the subject, creates a pull consumer (a shared
    /// durable for a queue group, or an ephemeral consumer for fan-out) positioned at the configured start position,
    /// and launches the background loop that dispatches received messages to <paramref name="onMessage"/>.
    /// </summary>
    /// <param name="onMessage">The callback invoked per received delivery.</param>
    /// <param name="ct">A token to cancel startup.</param>
    /// <returns>A task that completes once the consumer has been created.</returns>
    public async Task StartAsync(Func<TransportDelivery, CancellationToken, Task> onMessage, CancellationToken ct)
    {
        _onMessage = onMessage;

        var jetStream = await _connection.GetJetStreamAsync(ct);
        // The stream is provisioned externally; resolve it (a clear error is thrown if none captures the subject).
        var stream = await NatsStreamValidator.ResolveStreamAsync(jetStream, _options.Subject, ct);

        var config = BuildConsumerConfig();
        var consumer = await jetStream.CreateOrUpdateConsumerAsync(stream, config, ct);

        // The consumer's start position is pinned at creation, so messages published after StartAsync returns are
        // captured (and buffered by JetStream) even before the consume loop begins pulling — no readiness race, no loss.
        _loop = Task.Run(() => RunLoopAsync(consumer, _stopCts.Token), CancellationToken.None);
    }

    /// <summary>
    /// Acknowledges the delivery to JetStream (acks the underlying message token), marking it as durably consumed.
    /// A no-op once the consumer has been disposed.
    /// </summary>
    /// <param name="delivery">The delivery to acknowledge.</param>
    /// <returns>A task that completes when the acknowledgement has been sent, or immediately if skipped.</returns>
    public async Task CompleteAsync(TransportDelivery delivery)
    {
        if (_isDisposed || delivery.Token is not INatsJSMsg<string> msg)
            return;

        try
        {
            await msg.AckAsync();
        }
        catch (Exception e)
        {
            // The consumer stopped between the guard and the ack (dispose race); the message is redelivered later.
            this.Trace<string>("nats ack skipped: {error}", e.Message);
        }
    }

    /// <summary>
    /// Negatively acknowledges the delivery to JetStream (naks the underlying message token), triggering immediate
    /// raw redelivery. A no-op once the consumer has been disposed.
    /// </summary>
    /// <param name="delivery">The delivery to abandon.</param>
    /// <returns>A task that completes when the negative acknowledgement has been sent, or immediately if skipped.</returns>
    public async Task AbandonAsync(TransportDelivery delivery)
    {
        if (_isDisposed || delivery.Token is not INatsJSMsg<string> msg)
            return;

        try
        {
            // Negative ack → immediate redelivery (raw redelivery under at-least-once).
            await msg.NakAsync();
        }
        catch (Exception e)
        {
            this.Trace<string>("nats nak skipped: {error}", e.Message);
        }
    }

    /// <summary>
    /// The consume loop: pulls messages from the JetStream consumer and dispatches them to the pipeline callback until
    /// stopped. Handler faults are logged (the pipeline has already handled them) so the loop survives.
    /// </summary>
    /// <param name="consumer">The JetStream consumer to pull from.</param>
    /// <param name="ct">The loop cancellation token.</param>
    /// <returns>A task that completes when the loop stops.</returns>
    private async Task RunLoopAsync(INatsJSConsumer consumer, CancellationToken ct)
    {
        // Outer resilience loop: ConsumeAsync can throw a non-cancellation transport/consumer fault (e.g. a 503 storm,
        // a protocol hiccup, a transient connection failure). Rather than letting a single fault end consumption
        // permanently, log and re-establish the pull — mirroring the Kafka poll loop's per-iteration resilience.
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await foreach (var msg in consumer.ConsumeAsync<string>(cancellationToken: ct))
                {
                    var delivery = ToDelivery(msg);
                    try
                    {
                        await _onMessage!(delivery, ct);
                    }
                    catch (Exception e)
                    {
                        this.Error(e);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break; // stopping — expected on dispose
            }
            catch (Exception e)
            {
                this.Error(e);
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), ct);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Builds the pull-consumer configuration from the subscription options: explicit acks, prefetch as the
    /// unacknowledged-window bound, the subject filter (NATS-native wildcards), a durable name for a queue group
    /// (competing) or an ephemeral consumer for fan-out, and the deliver policy derived from the start position.
    /// </summary>
    /// <returns>The consumer configuration.</returns>
    private ConsumerConfig BuildConsumerConfig()
    {
        var config = new ConsumerConfig
        {
            AckPolicy = ConsumerConfigAckPolicy.Explicit,
            // A delivery stays un-acked in the broker while the shared pipeline runs its in-process retry loop (it does
            // not nak between attempts). AckWait must therefore exceed the worst-case retry duration, else JetStream
            // would redeliver a message still mid-retry — to another puller when Prefetch > 1 (a duplicate concurrent
            // handler invocation). Derive it from the retry budget rather than hardcoding.
            AckWait = ComputeAckWait(),
            // Prefetch bounds the number of unacknowledged in-flight deliveries (flow control); at 1 this also enforces
            // strictly ordered one-at-a-time delivery.
            MaxAckPending = _options.Prefetch,
            FilterSubject = _options.Subject,
        };

        if (_options.Group is { } group)
        {
            // Shared durable consumer → members of the group compete for messages.
            config.DurableName = SanitizeName($"{group}.{_options.Subject}");
        }
        else
        {
            // Ephemeral consumer (unique per subscription) → fan-out. An explicit name is required so the client builds
            // a well-formed create request; the consumer is reaped by the server after its inactivity threshold.
            config.Name = $"mb_{Guid.NewGuid():N}";
            config.InactiveThreshold = TimeSpan.FromMinutes(5);
        }

        _startPosition.Switch(
            onNew: () => config.DeliverPolicy = ConsumerConfigDeliverPolicy.New,
            onEarliest: () => config.DeliverPolicy = ConsumerConfigDeliverPolicy.All,
            onTimestamp: timestamp =>
            {
                config.DeliverPolicy = ConsumerConfigDeliverPolicy.ByStartTime;
                config.OptStartTime = timestamp;
            },
            onPosition: position =>
            {
                config.DeliverPolicy = ConsumerConfigDeliverPolicy.ByStartSequence;
                config.OptStartSeq = (ulong)position;
            }
        );

        return config;
    }

    /// <summary>
    /// Computes the JetStream ack-wait: long enough to outlast the shared pipeline's worst-case in-process retry
    /// duration (full, un-jittered exponential backoff across all attempts) plus a handler-execution headroom (the
    /// drain timeout, which bounds a single in-flight handler), floored at 30 seconds. This prevents JetStream from
    /// redelivering a message that is still being retried in this process.
    /// </summary>
    /// <returns>The ack-wait duration.</returns>
    private TimeSpan ComputeAckWait()
    {
        var retry = _options.Retry;
        var backoff = TimeSpan.Zero;
        var delayMs = retry.BaseDelay.TotalMilliseconds;
        // The pipeline delays before each retry: MaxAttempts - 1 gaps (jitter only shortens, so full backoff is worst).
        for (var i = 1; i < retry.MaxAttempts; i++)
        {
            backoff += TimeSpan.FromMilliseconds(Math.Min(delayMs, retry.MaxDelay.TotalMilliseconds));
            delayMs *= retry.Factor;
        }

        var ackWait = backoff + _options.StopTimeout + TimeSpan.FromSeconds(5);
        var floor = TimeSpan.FromSeconds(30);
        return ackWait < floor ? floor : ackWait;
    }

    /// <summary>
    /// Builds a <see cref="TransportDelivery"/> from a JetStream message, carrying the message itself as the
    /// acknowledgement token.
    /// </summary>
    /// <param name="msg">The received JetStream message.</param>
    /// <returns>The transport delivery.</returns>
    private static TransportDelivery ToDelivery(INatsJSMsg<string> msg)
    {
        var headers = NatsHeaderMapper.FromNatsHeaders(msg.Headers);
        var message = new TransportMessage(msg.Subject, msg.Data ?? string.Empty, headers, Key: null);
        return new TransportDelivery(message, msg);
    }

    /// <summary>
    /// Sanitizes a subject-derived string into a valid NATS consumer (durable) name by replacing every character that
    /// is not a letter, digit, '-' or '_' with '_'.
    /// </summary>
    /// <param name="value">The raw name.</param>
    /// <returns>The sanitized name.</returns>
    private static string SanitizeName(string value)
    {
        var sb = new StringBuilder(value.Length);
        foreach (var c in value)
            sb.Append(char.IsAsciiLetterOrDigit(c) || c is '-' or '_' ? c : '_');
        return sb.ToString();
    }

    /// <summary>
    /// Stops the consume loop, waits for it to exit (so the pull subscription is torn down) and flushes that teardown
    /// to the server, so no delivery is routed to this member once disposal returns. Idempotent. An ephemeral consumer
    /// is reaped by the server after its inactivity threshold; a durable (queue-group) consumer persists for peers.
    /// </summary>
    /// <returns>A task that completes when the consumer has stopped receiving.</returns>
    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
            return;
        _isDisposed = true;

        await _stopCts.CancelAsync();

        // Wait for the loop to exit: disposing its enumerator unsubscribes the pull inbox. Until that happens the
        // server still sees interest on this member's outstanding pull request and keeps routing messages into it —
        // messages nobody dispatches or acks, which only come back after AckWait and meanwhile consume the group's
        // MaxAckPending window (at Prefetch 1, a single such message stalls every peer). Bounded by StopTimeout: an
        // inline handler (Concurrency 1) runs on this loop, and the pipeline's own drain follows this call.
        try
        {
            await _loop.WaitAsync(_options.StopTimeout);
        }
        catch (TimeoutException)
        {
            this.Trace("nats consume loop did not stop within stop timeout");
        }
        catch (Exception e)
        {
            this.Error(e);
        }

        // Flush so the unsubscribe is processed server-side before disposal returns: the server drops a waiting pull
        // request whose reply inbox has no interest, handing its messages to the surviving group members at once.
        try
        {
            var connection = await _connection.GetConnectionAsync(CancellationToken.None);
            await connection.PingAsync();
        }
        catch (Exception e)
        {
            this.Trace<string>("nats flush on dispose skipped: {error}", e.Message);
        }

        // _stopCts is intentionally NOT disposed: its token was handed to the pipeline callback and a straggling
        // handler may still read it after this returns. An ephemeral consumer is reaped by the server after its
        // inactivity threshold, a durable (queue-group) consumer persists for peers.
    }
}
