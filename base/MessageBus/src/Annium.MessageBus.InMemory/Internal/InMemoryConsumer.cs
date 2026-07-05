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

    /// <inheritdoc />
    public ILogger Logger { get; }

    /// <inheritdoc />
    public Task StartAsync(Func<TransportDelivery, CancellationToken, Task> onMessage, CancellationToken ct)
    {
        // fire-and-forget: the loop observes its own faults (logs handler errors, swallows cancellation)
        _ = RunAsync(onMessage, _loopCts.Token);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task CompleteAsync(TransportDelivery delivery) => Task.CompletedTask;

    /// <inheritdoc />
    public Task AbandonAsync(TransportDelivery delivery)
    {
        // raw redelivery under at-least-once; drop under at-most-once
        if (_subscription.Delivery == DeliveryMode.AtLeastOnce && !_subscription.Writer.TryWrite(delivery.Message))
            this.Warn(
                "failed to redeliver message on {subject}: subscription channel closed",
                (object)delivery.Message.Subject
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

    /// <inheritdoc />
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
