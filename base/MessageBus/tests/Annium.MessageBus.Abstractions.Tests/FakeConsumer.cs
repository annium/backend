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

    /// <inheritdoc />
    public Task StartAsync(Func<TransportDelivery, CancellationToken, Task> onMessage, CancellationToken ct)
    {
        _onMessage = onMessage;
        _started = true;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task CompleteAsync(TransportDelivery delivery)
    {
        _transport.OnCompleted();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        _started = false;
        _transport.Remove(this);
        return ValueTask.CompletedTask;
    }
}
