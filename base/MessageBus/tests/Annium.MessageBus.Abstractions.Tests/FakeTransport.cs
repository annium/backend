using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.MessageBus.Abstractions.Tests;

/// <summary>
/// A minimal in-memory transport implementing the public message-bus SPI, used to drive the shared pipelines through
/// the public <see cref="IMessagePublisher"/>/<see cref="IMessageSubscriber"/> API (no <c>InternalsVisibleTo</c>).
/// Produced messages are routed synchronously to started consumers whose subscription subject matches.
/// </summary>
public sealed class FakeTransport : ITransportProducer, ITransportConsumerFactory
{
    /// <summary>
    /// The started consumers available for routing.
    /// </summary>
    private readonly List<FakeConsumer> _consumers = new();

    /// <summary>
    /// Aggregate count of transport-level completions (acks/commits).
    /// </summary>
    private int _completed;

    /// <summary>
    /// Aggregate count of transport-level abandonments.
    /// </summary>
    private int _abandoned;

    /// <summary>
    /// Gets the messages produced through this transport (including dead-letter messages).
    /// </summary>
    public List<TransportMessage> Produced { get; } = new();

    /// <summary>
    /// Gets the number of messages completed (acked/committed) at the transport level.
    /// </summary>
    public int Completed => Volatile.Read(ref _completed);

    /// <summary>
    /// Gets the number of messages abandoned (left unconfirmed) at the transport level.
    /// </summary>
    public int Abandoned => Volatile.Read(ref _abandoned);

    /// <summary>
    /// Gets the last exception surfaced from a consumer callback (mirrors an adapter's consumer-loop error handling).
    /// </summary>
    public Exception? LastConsumerError { get; private set; }

    /// <inheritdoc />
    public async Task ProduceAsync(TransportMessage message, CancellationToken ct)
    {
        lock (Produced)
            Produced.Add(message);

        FakeConsumer[] targets;
        lock (_consumers)
            targets = _consumers.Where(c => c.Matches(message.Subject)).ToArray();

        foreach (var consumer in targets)
            await consumer.DeliverAsync(message);
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
        var consumer = new FakeConsumer(this, options);
        lock (_consumers)
            _consumers.Add(consumer);
        return consumer;
    }

    /// <summary>
    /// Gets the messages produced to the dead-letter subject of the given subject.
    /// </summary>
    /// <param name="subject">The original subject.</param>
    /// <returns>The dead-letter messages.</returns>
    public IReadOnlyList<TransportMessage> Dlq(string subject)
    {
        lock (Produced)
            return Produced.Where(m => m.Subject == $"{subject}.dlq").ToArray();
    }

    /// <summary>
    /// Removes a consumer from routing.
    /// </summary>
    /// <param name="consumer">The consumer to remove.</param>
    internal void Remove(FakeConsumer consumer)
    {
        lock (_consumers)
            _consumers.Remove(consumer);
    }

    /// <summary>
    /// Records a transport-level completion.
    /// </summary>
    internal void OnCompleted() => Interlocked.Increment(ref _completed);

    /// <summary>
    /// Records a transport-level abandonment.
    /// </summary>
    internal void OnAbandoned() => Interlocked.Increment(ref _abandoned);

    /// <summary>
    /// Records a consumer-callback error.
    /// </summary>
    /// <param name="error">The error.</param>
    internal void OnConsumerError(Exception error) => LastConsumerError = error;
}
