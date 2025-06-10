using System;
using System.Text.Json.Serialization;

namespace Annium.Infrastructure.MessageBus.Node.Internal;

/// <summary>
/// Factory class for creating complex message envelopes that support multipart request/response scenarios.
/// TODO: use in case of request/response multipart envelopes
/// </summary>
internal static class MessageBusComplexEnvelope
{
    /// <summary>
    /// Creates a single envelope containing data that represents a complete message.
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    /// <param name="data">The data to include in the envelope.</param>
    /// <returns>A complex envelope containing the data marked as final.</returns>
    public static MessageBusComplexEnvelope<T> Single<T>(T data) =>
        new(Guid.NewGuid(), EnvelopeType.Data | EnvelopeType.Final, data, string.Empty);

    /// <summary>
    /// Creates a single envelope containing an error that represents a complete error response.
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    /// <param name="error">The error message to include in the envelope.</param>
    /// <returns>A complex envelope containing the error marked as final.</returns>
    public static MessageBusComplexEnvelope<T> Single<T>(string error) =>
        new(Guid.NewGuid(), EnvelopeType.Error | EnvelopeType.Final, default!, error);

    /// <summary>
    /// Creates a data chunk envelope for multipart message transmission.
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    /// <param name="id">The unique identifier for the multipart message series.</param>
    /// <param name="data">The data chunk to include in the envelope.</param>
    /// <returns>A complex envelope containing a data chunk.</returns>
    public static MessageBusComplexEnvelope<T> Chunk<T>(Guid id, T data) =>
        new(id, EnvelopeType.Data, data, string.Empty);

    /// <summary>
    /// Creates an error chunk envelope for multipart message transmission.
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    /// <param name="id">The unique identifier for the multipart message series.</param>
    /// <param name="error">The error message to include in the envelope.</param>
    /// <returns>A complex envelope containing an error chunk.</returns>
    public static MessageBusComplexEnvelope<T> Chunk<T>(Guid id, string error) =>
        new(id, EnvelopeType.Error, default!, error);

    /// <summary>
    /// Creates the final data envelope in a multipart message series.
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    /// <param name="id">The unique identifier for the multipart message series.</param>
    /// <param name="data">The final data to include in the envelope.</param>
    /// <returns>A complex envelope containing the final data marked as final.</returns>
    public static MessageBusComplexEnvelope<T> End<T>(Guid id, T data) =>
        new(id, EnvelopeType.Data | EnvelopeType.Final, data, string.Empty);

    /// <summary>
    /// Creates the final error envelope in a multipart message series.
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    /// <param name="id">The unique identifier for the multipart message series.</param>
    /// <param name="error">The error message to include in the envelope.</param>
    /// <returns>A complex envelope containing the final error marked as final.</returns>
    public static MessageBusComplexEnvelope<T> End<T>(Guid id, string error) =>
        new(id, EnvelopeType.Data | EnvelopeType.Final, default!, error);
}

/// <summary>
/// Represents a complex message envelope that supports multipart message transmission with type information and error handling.
/// </summary>
/// <typeparam name="T">The type of the data contained in the envelope.</typeparam>
internal class MessageBusComplexEnvelope<T>
{
    /// <summary>
    /// Gets the unique identifier for this envelope, used to correlate multipart messages.
    /// </summary>
    [JsonPropertyName("i")]
    public Guid Id { get; }

    /// <summary>
    /// Gets the type of this envelope, indicating whether it contains data or error and if it's final.
    /// </summary>
    [JsonPropertyName("t")]
    public EnvelopeType Type { get; }

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
    /// Initializes a new instance of the MessageBusComplexEnvelope class.
    /// </summary>
    /// <param name="id">The unique identifier for this envelope.</param>
    /// <param name="type">The type of the envelope.</param>
    /// <param name="data">The data to include in the envelope.</param>
    /// <param name="error">The error message to include in the envelope.</param>
    public MessageBusComplexEnvelope(Guid id, EnvelopeType type, T data, string error)
    {
        Id = id;
        Type = type;
        Data = data;
        Error = error;
    }
}

/// <summary>
/// Defines the types of envelope content and transmission state.
/// </summary>
[Flags]
internal enum EnvelopeType
{
    /// <summary>
    /// Indicates this is the final envelope in a multipart series.
    /// </summary>
    Final = 1,

    /// <summary>
    /// Indicates this envelope contains data.
    /// </summary>
    Data = 2,

    /// <summary>
    /// Indicates this envelope contains an error.
    /// </summary>
    Error = 4,
}
