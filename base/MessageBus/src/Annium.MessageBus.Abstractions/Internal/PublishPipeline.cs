using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Serialization.Abstractions;

namespace Annium.MessageBus.Abstractions.Internal;

/// <summary>
/// The shared publish engine: builds the headers-based envelope, serializes the payload via
/// <see cref="ISerializer{String}"/>, opens a producer span, and hands the message to the transport SPI. Adapters
/// wrap this behind their public <c>IMessagePublisher</c> implementations.
/// </summary>
internal sealed class PublishPipeline
{
    /// <summary>
    /// The default value carried in the version header.
    /// </summary>
    private const string DefaultVersion = "1";

    /// <summary>
    /// The transport producer messages are handed to.
    /// </summary>
    private readonly ITransportProducer _producer;

    /// <summary>
    /// The serializer used to turn payloads into transport bodies.
    /// </summary>
    private readonly ISerializer<string> _serializer;

    /// <summary>
    /// The content-type advertised in the envelope for produced messages.
    /// </summary>
    private readonly string _contentType;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublishPipeline"/> class.
    /// </summary>
    /// <param name="producer">The transport producer.</param>
    /// <param name="serializer">The payload serializer.</param>
    /// <param name="contentType">The content-type advertised in the envelope (defaults to <c>application/json</c>).</param>
    public PublishPipeline(
        ITransportProducer producer,
        ISerializer<string> serializer,
        string contentType = "application/json"
    )
    {
        _producer = producer;
        _serializer = serializer;
        _contentType = contentType;
    }

    /// <summary>
    /// Publishes a single message to the given subject.
    /// </summary>
    /// <typeparam name="T">The payload type.</typeparam>
    /// <param name="subject">The destination subject.</param>
    /// <param name="message">The payload.</param>
    /// <param name="options">Optional publish options (key, user headers).</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A task that completes when the message has been produced.</returns>
    public async Task PublishAsync<T>(
        string subject,
        T message,
        PublishOptions? options = null,
        CancellationToken ct = default
    )
    {
        using var activity = Diagnostics.StartPublish(subject);
        var transportMessage = BuildMessage(subject, message, options);
        await _producer.ProduceAsync(transportMessage, ct);
        Diagnostics.RecordPublish(subject);
    }

    /// <summary>
    /// Publishes a batch of messages to the given subject.
    /// </summary>
    /// <typeparam name="T">The payload type.</typeparam>
    /// <param name="subject">The destination subject.</param>
    /// <param name="messages">The payloads.</param>
    /// <param name="options">Optional publish options applied to every message (key, user headers).</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A task that completes when the batch has been produced.</returns>
    public async Task PublishBatchAsync<T>(
        string subject,
        IReadOnlyCollection<T> messages,
        PublishOptions? options = null,
        CancellationToken ct = default
    )
    {
        using var activity = Diagnostics.StartPublish(subject);
        var transportMessages = messages.Select(message => BuildMessage(subject, message, options)).ToArray();

        await _producer.ProduceBatchAsync(transportMessages, ct);
        foreach (var _ in transportMessages)
            Diagnostics.RecordPublish(subject);
    }

    /// <summary>
    /// Builds the transport message (serialized body + canonical envelope headers) for a payload.
    /// </summary>
    /// <typeparam name="T">The payload type.</typeparam>
    /// <param name="subject">The destination subject.</param>
    /// <param name="message">The payload.</param>
    /// <param name="options">Optional publish options.</param>
    /// <returns>The built transport message.</returns>
    private TransportMessage BuildMessage<T>(string subject, T message, PublishOptions? options)
    {
        var body = _serializer.Serialize(message);
        var headers = new Dictionary<string, string>(StringComparer.Ordinal);

        // user headers first so canonical envelope keys always take precedence
        if (options?.Headers is { } userHeaders)
            foreach (var (key, value) in userHeaders)
                headers[key] = value;

        // Id: honor a user-supplied id header, otherwise auto-generate
        if (!headers.TryGetValue(EnvelopeHeaders.Id, out var id) || string.IsNullOrEmpty(id))
        {
            id = Guid.NewGuid().ToString();
            headers[EnvelopeHeaders.Id] = id;
        }

        headers[EnvelopeHeaders.Type] = typeof(T).FullName ?? typeof(T).Name;
        headers[EnvelopeHeaders.Version] = DefaultVersion;
        headers[EnvelopeHeaders.ContentType] = _contentType;
        headers[EnvelopeHeaders.Timestamp] = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture);

        if (Activity.Current is { } activity)
        {
            headers[EnvelopeHeaders.TraceParent] = activity.Id ?? string.Empty;
            if (!string.IsNullOrEmpty(activity.TraceStateString))
                headers[EnvelopeHeaders.TraceState] = activity.TraceStateString;
        }

        return new TransportMessage(subject, body, headers, options?.Key);
    }
}
