using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Serialization.Abstractions;

namespace Annium.Infrastructure.MessageBus.Abstractions.Internal;

/// <summary>
/// The public <see cref="IMessageSubscriber"/> implementation. Creates one transport consumer per subscription via
/// <see cref="ITransportConsumerFactory"/> and wraps it in a shared <see cref="ConsumptionPipeline{T}"/>.
/// </summary>
internal sealed class MessageBusSubscriber : IMessageSubscriber
{
    /// <summary>
    /// The factory creating per-subscription transport consumers.
    /// </summary>
    private readonly ITransportConsumerFactory _consumerFactory;

    /// <summary>
    /// The transport producer used by the pipeline for dead-letter publishing.
    /// </summary>
    private readonly ITransportProducer _producer;

    /// <summary>
    /// The serializer used to deserialize payloads.
    /// </summary>
    private readonly ISerializer<string> _serializer;

    /// <summary>
    /// The logger passed to each pipeline.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageBusSubscriber"/> class.
    /// </summary>
    /// <param name="consumerFactory">The transport consumer factory.</param>
    /// <param name="producer">The transport producer (for dead-lettering).</param>
    /// <param name="serializer">The payload serializer.</param>
    /// <param name="logger">The logger.</param>
    public MessageBusSubscriber(
        ITransportConsumerFactory consumerFactory,
        ITransportProducer producer,
        ISerializer<string> serializer,
        ILogger logger
    )
    {
        _consumerFactory = consumerFactory;
        _producer = producer;
        _serializer = serializer;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IAsyncDisposable> SubscribeAsync<T>(
        SubscriptionOptions options,
        Func<IMessageContext<T>, CancellationToken, Task> handler
    )
        where T : notnull
    {
        var consumer = _consumerFactory.CreateConsumer(options);
        var pipeline = new ConsumptionPipeline<T>(consumer, _producer, _serializer, options, handler, _logger);
        await pipeline.StartAsync();
        return pipeline;
    }
}
