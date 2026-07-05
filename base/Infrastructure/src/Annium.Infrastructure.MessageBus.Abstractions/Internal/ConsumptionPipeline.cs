using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Serialization.Abstractions;

namespace Annium.Infrastructure.MessageBus.Abstractions.Internal;

/// <summary>
/// The shared consumption engine: deserializes the envelope, enforces the strict ack-contract, runs the in-process
/// exponential-backoff retry loop and dead-letter fallback, dispatches with bounded concurrency, and drains
/// in-flight work on disposal. Adapters wrap it behind their public <c>IMessageSubscriber</c> implementations.
/// </summary>
/// <typeparam name="T">The deserialized message payload type.</typeparam>
internal sealed class ConsumptionPipeline<T> : IAsyncDisposable, ILogSubject
    where T : notnull
{
    /// <summary>
    /// The transport consumer supplying raw messages.
    /// </summary>
    private readonly ITransportConsumer _consumer;

    /// <summary>
    /// The transport producer used for dead-letter publishing.
    /// </summary>
    private readonly ITransportProducer _producer;

    /// <summary>
    /// The serializer used to deserialize payloads.
    /// </summary>
    private readonly ISerializer<string> _serializer;

    /// <summary>
    /// The subscription options.
    /// </summary>
    private readonly SubscriptionOptions _options;

    /// <summary>
    /// The retry policy taken from the subscription options.
    /// </summary>
    private readonly RetryPolicy _retry;

    /// <summary>
    /// The message handler.
    /// </summary>
    private readonly Func<IMessageContext<T>, CancellationToken, Task> _handler;

    /// <summary>
    /// The concurrency gate limiting simultaneous handler invocations.
    /// </summary>
    private readonly SemaphoreSlim _gate;

    /// <summary>
    /// The set of in-flight processing tasks (parallel dispatch path), tracked for graceful drain.
    /// </summary>
    private readonly ConcurrentDictionary<Task, byte> _inFlight = new();

    /// <summary>
    /// Cancellation source signaling a hard stop to in-flight handlers after the drain timeout.
    /// </summary>
    private readonly CancellationTokenSource _cts = new();

    /// <summary>
    /// Guards against repeated disposal and rejects new work once stopping.
    /// </summary>
    private volatile bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsumptionPipeline{T}"/> class.
    /// </summary>
    /// <param name="consumer">The transport consumer.</param>
    /// <param name="producer">The transport producer (for dead-lettering).</param>
    /// <param name="serializer">The payload serializer.</param>
    /// <param name="options">The subscription options.</param>
    /// <param name="handler">The message handler.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentException">Thrown when <see cref="SubscriptionOptions.Concurrency"/> exceeds
    /// <see cref="SubscriptionOptions.Prefetch"/>, or either is below one.</exception>
    public ConsumptionPipeline(
        ITransportConsumer consumer,
        ITransportProducer producer,
        ISerializer<string> serializer,
        SubscriptionOptions options,
        Func<IMessageContext<T>, CancellationToken, Task> handler,
        ILogger logger
    )
    {
        if (options.Prefetch < 1)
            throw new ArgumentException("Prefetch must be at least 1.", nameof(options));
        if (options.Concurrency < 1)
            throw new ArgumentException("Concurrency must be at least 1.", nameof(options));
        if (options.Concurrency > options.Prefetch)
            throw new ArgumentException(
                $"Concurrency ({options.Concurrency}) must not exceed Prefetch ({options.Prefetch}).",
                nameof(options)
            );

        _consumer = consumer;
        _producer = producer;
        _serializer = serializer;
        _options = options;
        _retry = options.Retry;
        _handler = handler;
        _gate = new SemaphoreSlim(options.Concurrency, options.Concurrency);
        Logger = logger;
    }

    /// <inheritdoc />
    public ILogger Logger { get; }

    /// <summary>
    /// Starts consuming messages from the transport.
    /// </summary>
    /// <param name="ct">A token to cancel startup.</param>
    /// <returns>A task that completes once consumption has started.</returns>
    public Task StartAsync(CancellationToken ct = default) => _consumer.StartAsync(OnIncomingAsync, ct);

    /// <summary>
    /// Transport callback: rejects new work once stopping, gates on concurrency, then processes the message either
    /// inline (Concurrency == 1, which preserves order) or on a tracked background task (Concurrency &gt; 1). The
    /// processing task is tracked in both cases so graceful drain honors it.
    /// </summary>
    /// <param name="incomingMessage">The received message.</param>
    /// <param name="ct">The transport cancellation token.</param>
    /// <returns>A task that completes when the message has been accepted for (Concurrency &gt; 1) or has finished
    /// (Concurrency == 1) processing.</returns>
    private async Task OnIncomingAsync(ITransportIncomingMessage incomingMessage, CancellationToken ct)
    {
        if (_isDisposed)
        {
            await incomingMessage.AbandonAsync();
            return;
        }

        await _gate.WaitAsync(_cts.Token);

        var task = ProcessAndReleaseAsync(incomingMessage);
        Track(task);

        // Concurrency == 1: await inline so the transport delivers strictly sequentially and in order, and so that
        // contract violations (missing/duplicate disposition) surface to the caller.
        if (_options.Concurrency == 1)
            await task;
    }

    /// <summary>
    /// Runs <see cref="ProcessAsync"/> and always releases the concurrency gate afterwards.
    /// </summary>
    /// <param name="incomingMessage">The received message.</param>
    /// <returns>A task that completes when processing has finished and the gate has been released.</returns>
    private async Task ProcessAndReleaseAsync(ITransportIncomingMessage incomingMessage)
    {
        try
        {
            await ProcessAsync(incomingMessage);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// Runs the full processing algorithm for a single message: deserialize, retry loop, ack-contract enforcement,
    /// and dead-letter fallback.
    /// </summary>
    /// <param name="incomingMessage">The received message.</param>
    /// <returns>A task that completes when the message has been fully handled.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the handler returns without an explicit disposition,
    /// or records more than one disposition.</exception>
    private async Task ProcessAsync(ITransportIncomingMessage incomingMessage)
    {
        var id = incomingMessage.Headers.GetValueOrDefault(EnvelopeHeaders.Id) ?? string.Empty;
        var timestamp = ParseTimestamp(incomingMessage.Headers);
        var payload = _serializer.Deserialize<T>(incomingMessage.Body);

        DateTimeOffset? firstFailedAt = null;
        var attempt = 0;
        while (true)
        {
            attempt++;
            var context = new MessageContext<T>(id, incomingMessage.Headers, timestamp, payload);
            using var activity = Diagnostics.StartConsume(incomingMessage.Subject, id);
            var startedAt = Stopwatch.GetTimestamp();
            try
            {
                Diagnostics.RecordConsume(incomingMessage.Subject);
                await _handler(context, _cts.Token);
            }
            catch (Exception e) when (context.Disposition == Disposition.None)
            {
                // Handler faulted without acking/nacking: log the original exception and leave the message
                // unconfirmed (raw redelivery by the transport). The retry policy is deliberately NOT engaged.
                this.Error(e);
                await incomingMessage.AbandonAsync();
                return;
            }
            catch (Exception e)
            {
                // Handler recorded a disposition and then threw (duplicate ack/nack, or a fault after acking):
                // abandon to avoid leaving the message in limbo, then rethrow so the violation surfaces / is observed.
                this.Error(e);
                await incomingMessage.AbandonAsync();
                throw;
            }
            finally
            {
                Diagnostics.RecordConsumeLatency(
                    incomingMessage.Subject,
                    Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds
                );
            }

            switch (context.Disposition)
            {
                case Disposition.Ack:
                    await incomingMessage.CompleteAsync();
                    Diagnostics.RecordAck(incomingMessage.Subject);
                    return;

                case Disposition.Nack:
                    Diagnostics.RecordNack(incomingMessage.Subject);
                    firstFailedAt ??= DateTimeOffset.UtcNow;
                    if (context.NackRequeue && attempt < _retry.MaxAttempts)
                    {
                        Diagnostics.RecordRetry(incomingMessage.Subject);
                        await Task.Delay(GetBackoff(attempt), _cts.Token);
                        continue;
                    }

                    await PublishDlqAsync(incomingMessage, attempt, firstFailedAt.Value);
                    await incomingMessage.CompleteAsync();
                    Diagnostics.RecordDlq(incomingMessage.Subject);
                    return;

                default:
                    // Handler returned normally without recording a disposition: contract violation.
                    await incomingMessage.AbandonAsync();
                    throw new InvalidOperationException(
                        $"Handler for message '{id}' on '{incomingMessage.Subject}' returned without calling Ack/Nack."
                    );
            }
        }
    }

    /// <summary>
    /// Publishes the message to its dead-letter subject (<c>&lt;subject&gt;.dlq</c>) with diagnostic headers.
    /// </summary>
    /// <param name="incomingMessage">The original message.</param>
    /// <param name="attempts">The number of processing attempts made.</param>
    /// <param name="firstFailedAt">The timestamp of the first failure.</param>
    /// <returns>A task that completes when the dead-letter message has been produced.</returns>
    private async Task PublishDlqAsync(
        ITransportIncomingMessage incomingMessage,
        int attempts,
        DateTimeOffset firstFailedAt
    )
    {
        var headers = new Dictionary<string, string>(incomingMessage.Headers, StringComparer.Ordinal)
        {
            [EnvelopeHeaders.DeathReason] = $"Nacked after {attempts} attempt(s).",
            [EnvelopeHeaders.OriginalSubject] = incomingMessage.Subject,
            [EnvelopeHeaders.Attempts] = attempts.ToString(CultureInfo.InvariantCulture),
            [EnvelopeHeaders.FirstFailedAt] = firstFailedAt.ToString("O", CultureInfo.InvariantCulture),
        };

        var dlqMessage = new TransportMessage($"{incomingMessage.Subject}.dlq", incomingMessage.Body, headers, null);
        await _producer.ProduceAsync(dlqMessage, _cts.Token);
    }

    /// <summary>
    /// Computes the exponential-backoff delay for the given attempt, capped at the policy maximum with optional
    /// full jitter.
    /// </summary>
    /// <param name="attempt">The 1-based attempt number that just failed.</param>
    /// <returns>The delay before the next attempt.</returns>
    private TimeSpan GetBackoff(int attempt)
    {
        var ms = Math.Min(
            _retry.BaseDelay.TotalMilliseconds * Math.Pow(_retry.Factor, attempt - 1),
            _retry.MaxDelay.TotalMilliseconds
        );
        if (_retry.Jitter)
            ms *= Random.Shared.NextDouble();

        return TimeSpan.FromMilliseconds(ms);
    }

    /// <summary>
    /// Parses the publication timestamp from the envelope, falling back to now when absent or malformed.
    /// </summary>
    /// <param name="headers">The message headers.</param>
    /// <returns>The parsed timestamp.</returns>
    private static DateTimeOffset ParseTimestamp(IReadOnlyDictionary<string, string> headers)
    {
        if (
            headers.TryGetValue(EnvelopeHeaders.Timestamp, out var raw)
            && DateTimeOffset.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var ts)
        )
            return ts;

        return DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Adds a task to the in-flight set and schedules its removal on completion. Faults are observed and logged so a
    /// background (Concurrency &gt; 1) processing fault is never silently swallowed.
    /// </summary>
    /// <param name="task">The processing task to track.</param>
    private void Track(Task task)
    {
        _inFlight[task] = 0;
        _ = task.ContinueWith(
            t =>
            {
                _inFlight.TryRemove(t, out _);
                if (t.IsFaulted)
                    this.Error(t.Exception!);
            },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default
        );
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
            return;
        _isDisposed = true;

        // Stop new deliveries first, then drain in-flight handlers up to StopTimeout, then hard-cancel any stragglers.
        await _consumer.DisposeAsync();

        var inFlight = _inFlight.Keys.ToArray();
        if (inFlight.Length > 0)
            await Task.WhenAny(Task.WhenAll(inFlight), Task.Delay(_options.StopTimeout));

        await _cts.CancelAsync();
        _cts.Dispose();
    }
}
