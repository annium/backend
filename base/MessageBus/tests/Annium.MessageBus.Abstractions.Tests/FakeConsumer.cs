using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.MessageBus.Abstractions.Tests;

/// <summary>
/// An in-memory <see cref="ITransportConsumer"/> bound to a single subscription. Matches produced subjects against
/// the subscription's canonical pattern and invokes the pipeline callback, mirroring an adapter's consumer loop by
/// catching (rather than propagating) callback faults.
/// </summary>
public sealed class FakeConsumer : ITransportConsumer
{
    /// <summary>
    /// The owning transport.
    /// </summary>
    private readonly FakeTransport _transport;

    /// <summary>
    /// The compiled subscription subject pattern.
    /// </summary>
    private readonly SubjectPattern _pattern;

    /// <summary>
    /// The pipeline callback, once started.
    /// </summary>
    private Func<TransportDelivery, CancellationToken, Task>? _onMessage;

    /// <summary>
    /// Whether the consumer has started and may receive messages.
    /// </summary>
    private volatile bool _started;

    /// <summary>
    /// Initializes a new instance of the <see cref="FakeConsumer"/> class.
    /// </summary>
    /// <param name="transport">The owning transport.</param>
    /// <param name="options">The subscription options.</param>
    public FakeConsumer(FakeTransport transport, SubscriptionOptions options)
    {
        _transport = transport;
        _pattern = SubjectPattern.Parse(options.Subject);
    }

    /// <summary>
    /// Records <paramref name="onMessage"/> as the pipeline callback and marks this consumer as started, so it can
    /// begin matching and receiving produced subjects. Completes immediately (no real subscription is opened).
    /// </summary>
    /// <param name="onMessage">The callback to invoke for each delivered message.</param>
    /// <param name="ct">Unused; accepted to satisfy the interface.</param>
    /// <returns>A task that is already completed.</returns>
    public Task StartAsync(Func<TransportDelivery, CancellationToken, Task> onMessage, CancellationToken ct)
    {
        _onMessage = onMessage;
        _started = true;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Records a transport-level completion on the owning <see cref="FakeTransport"/> for assertions; the
    /// <paramref name="delivery"/> itself is not inspected.
    /// </summary>
    /// <param name="delivery">The delivery being acknowledged.</param>
    /// <returns>A task that is already completed.</returns>
    public Task CompleteAsync(TransportDelivery delivery)
    {
        _transport.OnCompleted();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Records a transport-level abandonment on the owning <see cref="FakeTransport"/> for assertions; the
    /// <paramref name="delivery"/> itself is not redelivered or dropped.
    /// </summary>
    /// <param name="delivery">The delivery being abandoned.</param>
    /// <returns>A task that is already completed.</returns>
    public Task AbandonAsync(TransportDelivery delivery)
    {
        _transport.OnAbandoned();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Returns whether this started consumer's pattern matches the given subject.
    /// </summary>
    /// <param name="subject">The subject to test.</param>
    /// <returns>True if the consumer should receive the subject.</returns>
    public bool Matches(string subject) => _started && _pattern.Matches(subject);

    /// <summary>
    /// Delivers a produced message to the pipeline callback.
    /// </summary>
    /// <param name="message">The produced message.</param>
    /// <returns>A task that completes when the callback has accepted (or finished) the message.</returns>
    public async Task DeliverAsync(TransportMessage message)
    {
        var handler = _onMessage;
        if (handler is null)
            return;

        try
        {
            await handler(new TransportDelivery(message), CancellationToken.None);
        }
        catch (Exception e)
        {
            // A real adapter's consumer loop catches pipeline faults so the loop survives; record for assertions.
            _transport.OnConsumerError(e);
        }
    }

    /// <summary>
    /// Marks the consumer as stopped (so it no longer matches produced subjects) and removes it from the owning
    /// <see cref="FakeTransport"/>'s routing list.
    /// </summary>
    /// <returns>A task that is already completed.</returns>
    public ValueTask DisposeAsync()
    {
        _started = false;
        _transport.Remove(this);
        return ValueTask.CompletedTask;
    }
}
