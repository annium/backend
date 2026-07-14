using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.MessageBus.Abstractions;

namespace Annium.MessageBus.InMemory.Internal;

/// <summary>
/// An in-memory consumer bound to a single subscription. Runs a background read loop over the subscription channel,
/// invoking the pipeline callback per message. Competing consumers of the same group each run their own loop over
/// the shared channel, so each message is handled by exactly one of them.
/// </summary>
internal sealed class InMemoryConsumer : ITransportConsumer, ILogSubject
{
    /// <summary>
    /// The owning transport.
    /// </summary>
    private readonly InMemoryTransport _transport;

    /// <summary>
    /// The subscription this consumer reads from.
    /// </summary>
    private readonly InMemorySubscription _subscription;

    /// <summary>
    /// Cancellation source stopping the read loop (does not interrupt an in-flight handler — the pipeline bounds
    /// that with its own drain timeout).
    /// </summary>
    private readonly CancellationTokenSource _loopCts = new();

    /// <summary>
    /// Guards against repeated disposal.
    /// </summary>
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryConsumer"/> class.
    /// </summary>
    /// <param name="transport">The owning transport.</param>
    /// <param name="subscription">The subscription to read from.</param>
    /// <param name="logger">The logger.</param>
    public InMemoryConsumer(InMemoryTransport transport, InMemorySubscription subscription, ILogger logger)
    {
        _transport = transport;
        _subscription = subscription;
        Logger = logger;
    }

    /// <summary>
    /// Gets the logger used for diagnostic output by this consumer.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Launches the background read loop over the subscription channel, invoking <paramref name="onMessage"/> for
    /// each delivered message. The loop runs fire-and-forget and is stopped via <see cref="DisposeAsync"/> rather
    /// than through cancellation of this call.
    /// </summary>
    /// <param name="onMessage">The pipeline callback invoked per received delivery.</param>
    /// <param name="ct">Unused; the read loop is cancelled via <see cref="DisposeAsync"/> instead.</param>
    /// <returns>A completed task; the read loop has been launched but runs independently.</returns>
    public Task StartAsync(Func<TransportDelivery, CancellationToken, Task> onMessage, CancellationToken ct)
    {
        // fire-and-forget: the loop observes its own faults (logs handler errors, swallows cancellation)
        _ = RunAsync(onMessage, _loopCts.Token);
        return Task.CompletedTask;
    }

    /// <summary>
    /// No-op: the in-memory transport has no acknowledgement mechanism, so completion is always a no-op.
    /// </summary>
    /// <param name="delivery">The delivery to acknowledge (unused).</param>
    /// <returns>A completed task.</returns>
    public Task CompleteAsync(TransportDelivery delivery) => Task.CompletedTask;

    /// <summary>
    /// Abandons the delivery: under <see cref="DeliveryMode.AtLeastOnce"/> the message is written back onto the
    /// subscription channel for raw redelivery; under <see cref="DeliveryMode.AtMostOnce"/> it is dropped.
    /// </summary>
    /// <param name="delivery">The delivery to abandon.</param>
    /// <returns>A completed task.</returns>
    public Task AbandonAsync(TransportDelivery delivery)
    {
        // raw redelivery under at-least-once; drop under at-most-once
        if (_subscription.Delivery == DeliveryMode.AtLeastOnce && !_subscription.Writer.TryWrite(delivery.Message))
            this.Error<string>(
                "failed to redeliver message on {subject}: subscription channel closed",
                delivery.Message.Subject
            );

        return Task.CompletedTask;
    }

    /// <summary>
    /// The read loop: drains the subscription channel and invokes the pipeline callback per message. Handler faults
    /// are logged (the pipeline has already handled them) so the loop survives.
    /// </summary>
    /// <param name="onMessage">The pipeline callback.</param>
    /// <param name="loopCt">The loop cancellation token.</param>
    /// <returns>A task that completes when the loop stops.</returns>
    private async Task RunAsync(Func<TransportDelivery, CancellationToken, Task> onMessage, CancellationToken loopCt)
    {
        var reader = _subscription.Reader;
        try
        {
            while (await reader.WaitToReadAsync(loopCt))
            {
                while (reader.TryRead(out var message))
                {
                    try
                    {
                        await onMessage(new TransportDelivery(message), loopCt);
                    }
                    catch (Exception e)
                    {
                        this.Error(e);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // loop cancelled on dispose — normal stop
        }
    }

    /// <summary>
    /// Stops the read loop and releases this consumer's hold on the subscription, completing the subscription
    /// channel once the last consumer has left.
    /// </summary>
    /// <returns>A task that completes once the read loop has been cancelled and the subscription released.</returns>
    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
            return;
        _isDisposed = true;

        // Stop reading new messages; the pipeline's own DisposeAsync drains the in-flight handler up to StopTimeout.
        await _loopCts.CancelAsync();
        _transport.Release(_subscription);
        _loopCts.Dispose();
    }
}
