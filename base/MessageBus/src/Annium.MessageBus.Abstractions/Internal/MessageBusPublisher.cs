using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Serialization.Abstractions;

namespace Annium.MessageBus.Abstractions.Internal;

/// <summary>
/// The public <see cref="IMessagePublisher"/> implementation. Resolves the transport producer and serializer from
/// DI and delegates envelope building / producing to the shared <see cref="PublishPipeline"/>.
/// </summary>
internal sealed class MessageBusPublisher : IMessagePublisher
{
    /// <summary>
    /// The shared publishing engine.
    /// </summary>
    private readonly PublishPipeline _pipeline;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageBusPublisher"/> class.
    /// </summary>
    /// <param name="producer">The transport producer.</param>
    /// <param name="serializer">The payload serializer.</param>
    public MessageBusPublisher(ITransportProducer producer, ISerializer<string> serializer)
    {
        _pipeline = new PublishPipeline(producer, serializer);
    }

    /// <inheritdoc />
    public Task PublishAsync<T>(string subject, T message, PublishOptions? options = null)
        where T : notnull => _pipeline.PublishAsync(subject, message, options);

    /// <inheritdoc />
    public Task PublishAsync<T>(T message, PublishOptions? options = null)
        where T : ISubjectAware => _pipeline.PublishAsync(T.Subject, message, options);

    /// <inheritdoc />
    public Task PublishBatchAsync<T>(string subject, IReadOnlyCollection<T> messages, PublishOptions? options = null)
        where T : notnull => _pipeline.PublishBatchAsync(subject, messages, options);

    /// <inheritdoc />
    public Task PublishBatchAsync<T>(IReadOnlyCollection<T> messages, PublishOptions? options = null)
        where T : ISubjectAware => _pipeline.PublishBatchAsync(T.Subject, messages, options);
}
