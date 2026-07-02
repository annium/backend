using System.Collections.Generic;
using System.Threading.Tasks;

namespace Annium.Infrastructure.MessageBus.Abstractions;

/// <summary>
/// Publishes messages to subjects.
/// </summary>
public interface IMessagePublisher
{
    /// <summary>
    /// Publishes a message to the given subject.
    /// </summary>
    /// <typeparam name="T">The message type.</typeparam>
    /// <param name="subject">The canonical subject to publish to.</param>
    /// <param name="message">The message to publish.</param>
    /// <param name="options">Optional per-publish settings.</param>
    /// <returns>A task that completes when the broker has accepted the message (per the delivery mode).</returns>
    Task PublishAsync<T>(string subject, T message, PublishOptions? options = null)
        where T : notnull;

    /// <summary>
    /// Publishes a message to the subject declared by its type via <see cref="ISubjectAware"/>.
    /// </summary>
    /// <typeparam name="T">The subject-aware message type.</typeparam>
    /// <param name="message">The message to publish.</param>
    /// <param name="options">Optional per-publish settings.</param>
    /// <returns>A task that completes when the broker has accepted the message (per the delivery mode).</returns>
    Task PublishAsync<T>(T message, PublishOptions? options = null)
        where T : ISubjectAware;

    /// <summary>
    /// Publishes a batch of messages to the given subject.
    /// </summary>
    /// <typeparam name="T">The message type.</typeparam>
    /// <param name="subject">The canonical subject to publish to.</param>
    /// <param name="messages">The messages to publish.</param>
    /// <param name="options">Optional per-publish settings applied to every message.</param>
    /// <returns>A task that completes when the broker has accepted the batch (per the delivery mode).</returns>
    Task PublishBatchAsync<T>(string subject, IReadOnlyCollection<T> messages, PublishOptions? options = null)
        where T : notnull;

    /// <summary>
    /// Publishes a batch of messages to the subject declared by their type via <see cref="ISubjectAware"/>.
    /// </summary>
    /// <typeparam name="T">The subject-aware message type.</typeparam>
    /// <param name="messages">The messages to publish.</param>
    /// <param name="options">Optional per-publish settings applied to every message.</param>
    /// <returns>A task that completes when the broker has accepted the batch (per the delivery mode).</returns>
    Task PublishBatchAsync<T>(IReadOnlyCollection<T> messages, PublishOptions? options = null)
        where T : ISubjectAware;
}
