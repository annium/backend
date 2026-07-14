using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Serialization.Abstractions;

namespace Annium.MessageBus.Abstractions.Internal;

/// <summary>
/// The replay-capable subscriber, registered by adapters whose transport supports replay (e.g. Kafka, NATS). It is the
/// same singleton as the resolved <see cref="IMessageSubscriber"/>, additionally implementing
/// <see cref="IReplayableMessageSubscriber"/> (injectable as a superset, or detectable via <c>is</c>). Replay
/// subscriptions flow through the shared <see cref="MessageBusSubscriber.SubscribeInternalAsync{T}"/>; the adapter reads
/// the <see cref="StartPosition"/> from the <see cref="ReplaySubscriptionOptions"/> when creating the consumer.
/// </summary>
internal sealed class ReplayableMessageBusSubscriber : MessageBusSubscriber, IReplayableMessageSubscriber
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReplayableMessageBusSubscriber"/> class.
    /// </summary>
    /// <param name="consumerFactory">The transport consumer factory.</param>
    /// <param name="producer">The transport producer (for dead-lettering).</param>
    /// <param name="serializer">The payload serializer.</param>
    /// <param name="logger">The logger.</param>
    public ReplayableMessageBusSubscriber(
        ITransportConsumerFactory consumerFactory,
        ITransportProducer producer,
        ISerializer<string> serializer,
        ILogger logger
    )
        : base(consumerFactory, producer, serializer, logger) { }

    /// <summary>
    /// Subscribes with a start position (replay). See <see cref="IMessageSubscriber.SubscribeAsync{T}"/> for
    /// consumption and acknowledgement semantics.
    /// </summary>
    /// <typeparam name="T">The message payload type.</typeparam>
    /// <param name="options">The replay subscription settings, including the start position.</param>
    /// <param name="handler">The per-message handler.</param>
    /// <returns>A task yielding a disposable that stops the subscription (graceful drain on dispose).</returns>
    public Task<IAsyncDisposable> SubscribeAsync<T>(
        ReplaySubscriptionOptions options,
        Func<IMessageContext<T>, CancellationToken, Task> handler
    )
        where T : notnull => SubscribeInternalAsync(options, handler);
}
