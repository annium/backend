using System;
using System.Text.Json.Serialization;

namespace Annium.Infrastructure.MessageBus.Node.Internal;

/// <summary>
/// Factory class for creating simple message envelopes for request-response communication.
/// </summary>
internal static class MessageBusEnvelope
{
    /// <summary>
    /// Creates a data envelope with the specified identifier and data.
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    /// <param name="id">The unique identifier for the message.</param>
    /// <param name="data">The data to include in the envelope.</param>
    /// <returns>A message envelope containing the data.</returns>
    public static MessageBusEnvelope<T> Data<T>(Guid id, T data) => new(id, data, string.Empty);

    /// <summary>
    /// Creates an error envelope with the specified identifier and error message.
    /// </summary>
    /// <param name="id">The unique identifier for the message.</param>
    /// <param name="error">The error message to include in the envelope.</param>
    /// <returns>A message envelope containing the error.</returns>
    public static MessageBusEnvelope<object> Error(Guid id, string error) => new(id, default!, error);
}

/// <summary>
/// Represents a simple message envelope for request-response communication with error handling.
/// </summary>
/// <typeparam name="T">The type of the data contained in the envelope.</typeparam>
internal class MessageBusEnvelope<T>
{
    /// <summary>
    /// Gets the unique identifier for this message envelope.
    /// </summary>
    [JsonPropertyName("i")]
    public Guid Id { get; }

    /// <summary>
    /// Gets the data contained in this envelope.
    /// </summary>
    [JsonPropertyName("d")]
    public T Data { get; }

    /// <summary>
    /// Gets the error message contained in this envelope, if any.
    /// </summary>
    [JsonPropertyName("e")]
    public string Error { get; }

    /// <summary>
    /// Gets a value indicating whether this envelope represents a successful response (no error).
    /// </summary>
    public bool IsSuccess => string.IsNullOrWhiteSpace(Error);

    /// <summary>
    /// Initializes a new instance of the MessageBusEnvelope class.
    /// </summary>
    /// <param name="id">The unique identifier for this envelope.</param>
    /// <param name="data">The data to include in the envelope.</param>
    /// <param name="error">The error message to include in the envelope.</param>
    public MessageBusEnvelope(Guid id, T data, string error)
    {
        Id = id;
        Data = data;
        Error = error;
    }
}
