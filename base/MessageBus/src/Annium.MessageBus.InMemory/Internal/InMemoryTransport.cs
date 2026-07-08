using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.MessageBus.Abstractions;

namespace Annium.MessageBus.InMemory.Internal;

/// <summary>
/// The in-memory transport: a single in-process broker implementing the producer and consumer-factory SPI over
/// <see cref="System.Threading.Channels"/>. Registered as a singleton by <c>AddInMemoryMessageBus</c>.
/// </summary>
internal sealed class InMemoryTransport : ITransportProducer, ITransportConsumerFactory, IAsyncDisposable, ILogSubject
{
    /// <inheritdoc />
    public ILogger Logger { get; }

    /// <summary>
    /// The live subscriptions, guarded by locking on the list itself.
    /// </summary>
    private readonly List<InMemorySubscription> _subscriptions = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryTransport"/> class.
    /// </summary>
    /// <param name="logger">The logger passed to consumers.</param>
    public InMemoryTransport(ILogger logger)
    {
        Logger = logger;
    }

    /// <inheritdoc />
    public Task ProduceAsync(TransportMessage message, CancellationToken ct)
    {
        InMemorySubscription[] targets;
        lock (_subscriptions)
            targets = _subscriptions.Where(s => s.Pattern.Matches(message.Subject)).ToArray();

        // one write per matching subscription: competing within a group's channel, fan-out across subscriptions.
        foreach (var subscription in targets)
            if (!subscription.Writer.TryWrite(message))
                this.Trace<string>("dropped message on {subject}: subscription channel closed", message.Subject);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task ProduceBatchAsync(IReadOnlyCollection<TransportMessage> messages, CancellationToken ct)
    {
        foreach (var message in messages)
            await ProduceAsync(message, ct);
    }

    /// <inheritdoc />
    public ITransportConsumer CreateConsumer(SubscriptionOptions options)
    {
        var pattern = SubjectPattern.Parse(options.Subject);

        InMemorySubscription subscription;
        lock (_subscriptions)
        {
            var existing = options.Group is { } group
                ? _subscriptions.FirstOrDefault(s => s.Group == group && s.Key == options.Subject)
                : null;

            if (existing is null)
            {
                subscription = new InMemorySubscription(pattern, options.Subject, options.Group, options.Delivery);
                _subscriptions.Add(subscription);
            }
            else
            {
                subscription = existing;
            }

            subscription.Readers++;
        }

        return new InMemoryConsumer(this, subscription, Logger);
    }

    /// <summary>
    /// Releases a consumer's hold on a subscription; removes and completes the subscription when the last consumer
    /// leaves.
    /// </summary>
    /// <param name="subscription">The subscription to release.</param>
    internal void Release(InMemorySubscription subscription)
    {
        lock (_subscriptions)
        {
            subscription.Readers--;
            if (subscription.Readers > 0)
                return;

            _subscriptions.Remove(subscription);
            subscription.Writer.TryComplete();
        }
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        lock (_subscriptions)
        {
            foreach (var subscription in _subscriptions)
                subscription.Writer.TryComplete();
            _subscriptions.Clear();
        }

        return ValueTask.CompletedTask;
    }
}
