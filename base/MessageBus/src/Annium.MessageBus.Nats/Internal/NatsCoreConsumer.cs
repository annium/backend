using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.MessageBus.Abstractions;
using NATS.Client.Core;

namespace Annium.MessageBus.Nats.Internal;

/// <summary>
/// A Core NATS transport consumer bound to a single at-most-once subscription. Subscribes to the subject (NATS-native
/// wildcards, optional queue group for competing consumers) and dispatches each received message to the pipeline
/// callback. Core NATS has no acknowledgement or redelivery, so <see cref="CompleteAsync"/> / <see cref="AbandonAsync"/>
/// are no-ops — a message is delivered at most once, matching <see cref="DeliveryMode.AtMostOnce"/>.
/// </summary>
internal sealed class NatsCoreConsumer : ITransportConsumer, ILogSubject
{
    /// <summary>
    /// The logger for this consumer.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The shared connection.
    /// </summary>
    private readonly NatsConnectionHolder _connection;

    /// <summary>
    /// The subscription options.
    /// </summary>
    private readonly SubscriptionOptions _options;

    /// <summary>
    /// Cancellation source signaling the consumer is stopping; its token is passed to the pipeline callback and stops
    /// the read loop.
    /// </summary>
    private readonly CancellationTokenSource _stopCts = new();

    /// <summary>
    /// The underlying Core subscription, created in <see cref="StartAsync"/>.
    /// </summary>
    private INatsSub<string>? _sub;

    /// <summary>
    /// The pipeline callback invoked per delivery.
    /// </summary>
    private Func<TransportDelivery, CancellationToken, Task>? _onMessage;

    /// <summary>
    /// Guards against repeated disposal.
    /// </summary>
    private volatile bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="NatsCoreConsumer"/> class.
    /// </summary>
    /// <param name="connection">The shared connection.</param>
    /// <param name="options">The subscription options.</param>
    /// <param name="logger">The logger.</param>
    public NatsCoreConsumer(NatsConnectionHolder connection, SubscriptionOptions options, ILogger logger)
    {
        _connection = connection;
        _options = options;
        Logger = logger;
    }

    /// <summary>
    /// Starts a Core NATS subscription on the configured subject (queue group if one is set, otherwise a plain
    /// fan-out subscription) and launches the background loop that dispatches received messages to
    /// <paramref name="onMessage"/>.
    /// </summary>
    /// <param name="onMessage">The callback invoked per received delivery.</param>
    /// <param name="ct">A token to cancel startup.</param>
    /// <returns>A task that completes once the subscription is established.</returns>
    public async Task StartAsync(Func<TransportDelivery, CancellationToken, Task> onMessage, CancellationToken ct)
    {
        _onMessage = onMessage;

        var connection = await _connection.GetConnectionAsync(ct);
        // Group set → a NATS queue group (competing consumers); Group null → a plain subscription (fan-out).
        _sub = await connection.SubscribeCoreAsync<string>(
            _options.Subject,
            queueGroup: _options.Group,
            cancellationToken: ct
        );

        // Round-trip a ping so the SUB protocol frame is processed server-side before StartAsync returns; a subsequent
        // publish is then guaranteed to be captured by this subscription (no readiness race).
        await connection.PingAsync(ct);

        _ = Task.Run(() => RunLoopAsync(_sub, _stopCts.Token), CancellationToken.None);
    }

    /// <summary>
    /// No-op: Core NATS has no acknowledgement or redelivery, so a delivery is already at-most-once by the time it
    /// reaches the pipeline.
    /// </summary>
    /// <param name="delivery">The delivery to acknowledge.</param>
    /// <returns>A completed task.</returns>
    public Task CompleteAsync(TransportDelivery delivery) => Task.CompletedTask;

    /// <summary>
    /// No-op: Core NATS has no acknowledgement or redelivery, so there is nothing to abandon.
    /// </summary>
    /// <param name="delivery">The delivery to abandon.</param>
    /// <returns>A completed task.</returns>
    public Task AbandonAsync(TransportDelivery delivery) => Task.CompletedTask;

    /// <summary>
    /// The read loop: pulls messages from the subscription channel and dispatches them to the pipeline callback until
    /// stopped. Handler faults are logged (the pipeline has already handled them) so the loop survives.
    /// </summary>
    /// <param name="sub">The subscription to read from.</param>
    /// <param name="ct">The loop cancellation token.</param>
    /// <returns>A task that completes when the loop stops.</returns>
    private async Task RunLoopAsync(INatsSub<string> sub, CancellationToken ct)
    {
        try
        {
            await foreach (var msg in sub.Msgs.ReadAllAsync(ct))
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
            // stopping — expected on dispose
        }
        catch (Exception e)
        {
            // A terminal fault on the subscription channel (the connection auto-recovers reconnects internally, so this
            // is a genuine end): log so the fire-and-forget loop never faults as an unobserved exception. At-most-once
            // is best-effort, so consumption simply ends rather than re-subscribing.
            this.Error(e);
        }
    }

    /// <summary>
    /// Builds a <see cref="TransportDelivery"/> from a Core NATS message. Core has no acknowledgement handle, so the
    /// token is null.
    /// </summary>
    /// <param name="msg">The received message.</param>
    /// <returns>The transport delivery.</returns>
    private static TransportDelivery ToDelivery(NatsMsg<string> msg)
    {
        var headers = NatsHeaderMapper.FromNatsHeaders(msg.Headers);
        var message = new TransportMessage(msg.Subject, msg.Data ?? string.Empty, headers, Key: null);
        return new TransportDelivery(message, Token: null);
    }

    /// <summary>
    /// Stops the read loop and disposes the underlying Core subscription. Idempotent; a failed subscription dispose
    /// is logged rather than thrown.
    /// </summary>
    /// <returns>A task that completes when disposal has finished.</returns>
    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
            return;
        _isDisposed = true;

        await _stopCts.CancelAsync();

        if (_sub is { } sub)
        {
            try
            {
                await sub.DisposeAsync();

                // Round-trip a ping so the UNSUB frame is processed server-side before dispose returns; the server then
                // stops routing to this member — otherwise a queue group would keep handing messages to a subscription
                // that is already gone, and at-most-once drops them.
                var connection = await _connection.GetConnectionAsync(CancellationToken.None);
                await connection.PingAsync();
            }
            catch (Exception e)
            {
                this.Error<string>("nats core subscription dispose failed: {error}", e.Message);
            }
        }

        // _stopCts is intentionally NOT disposed: its token was handed to the pipeline callback and a straggling
        // handler may still read it after this returns.
    }
}
