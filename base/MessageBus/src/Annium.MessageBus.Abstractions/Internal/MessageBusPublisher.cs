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

    /// <summary>
    /// Publishes a message to the given subject.
    /// </summary>
    /// <typeparam name="T">The message type.</typeparam>
    /// <param name="subject">The canonical subject to publish to.</param>
    /// <param name="message">The message to publish.</param>
    /// <param name="options">Optional per-publish settings.</param>
    /// <returns>A task that completes when the broker has accepted the message (per the delivery mode).</returns>
    public Task PublishAsync<T>(string subject, T message, PublishOptions? options = null)
        where T : notnull => _pipeline.PublishAsync(subject, message, options);

    /// <summary>
    /// Publishes a message to the subject declared by its type via <see cref="ISubjectAware"/>.
    /// </summary>
    /// <typeparam name="T">The subject-aware message type.</typeparam>
    /// <param name="message">The message to publish.</param>
    /// <param name="options">Optional per-publish settings.</param>
    /// <returns>A task that completes when the broker has accepted the message (per the delivery mode).</returns>
    public Task PublishAsync<T>(T message, PublishOptions? options = null)
        where T : ISubjectAware => _pipeline.PublishAsync(T.Subject, message, options);

    /// <summary>
    /// Publishes a batch of messages to the given subject.
    /// </summary>
    /// <typeparam name="T">The message type.</typeparam>
    /// <param name="subject">The canonical subject to publish to.</param>
    /// <param name="messages">The messages to publish.</param>
    /// <param name="options">Optional per-publish settings applied to every message.</param>
    /// <returns>A task that completes when the broker has accepted the batch (per the delivery mode).</returns>
    public Task PublishBatchAsync<T>(string subject, IReadOnlyCollection<T> messages, PublishOptions? options = null)
        where T : notnull => _pipeline.PublishBatchAsync(subject, messages, options);

    /// <summary>
    /// Publishes a batch of messages to the subject declared by their type via <see cref="ISubjectAware"/>.
    /// </summary>
    /// <typeparam name="T">The subject-aware message type.</typeparam>
    /// <param name="messages">The messages to publish.</param>
    /// <param name="options">Optional per-publish settings applied to every message.</param>
    /// <returns>A task that completes when the broker has accepted the batch (per the delivery mode).</returns>
    public Task PublishBatchAsync<T>(IReadOnlyCollection<T> messages, PublishOptions? options = null)
        where T : ISubjectAware => _pipeline.PublishBatchAsync(T.Subject, messages, options);
}
