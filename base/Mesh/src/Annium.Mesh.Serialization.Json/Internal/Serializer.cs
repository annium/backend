using System;
using System.Text.Json;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Mesh.Domain;
using Annium.Mesh.Serialization.Abstractions;
using Annium.Serialization.Abstractions;

namespace Annium.Mesh.Serialization.Json.Internal;

/// <summary>
/// Provides JSON serialization implementation for mesh messages and data payloads.
/// </summary>
internal class Serializer : ISerializer
{
    /// <summary>
    /// JSON serializer options configured specifically for mesh message serialization.
    /// </summary>
    private static readonly JsonSerializerOptions _messageOpts;

    /// <summary>
    /// Static constructor that initializes the JSON serializer options with the custom message converter.
    /// </summary>
    static Serializer()
    {
        _messageOpts = new JsonSerializerOptions();
        _messageOpts.Converters.Add(new MessageConverter());
    }

    /// <summary>
    /// The underlying serializer used for data payload serialization.
    /// </summary>
    private readonly ISerializer<ReadOnlyMemory<byte>> _serializer;

    /// <summary>
    /// Initializes a new instance of the Serializer class.
    /// </summary>
    /// <param name="sp">The service provider used to resolve the underlying serializer.</param>
    public Serializer(IServiceProvider sp)
    {
        var key = SerializerKey.Create(Constants.SerializerKey, Annium.Serialization.Json.Constants.MediaType);
        _serializer = sp.ResolveKeyed<ISerializer<ReadOnlyMemory<byte>>>(key);
    }

    /// <summary>
    /// Serializes a mesh message to JSON format as binary data.
    /// </summary>
    /// <param name="message">The message to serialize.</param>
    /// <returns>The serialized message as UTF-8 encoded JSON bytes.</returns>
    public ReadOnlyMemory<byte> SerializeMessage(Message message)
    {
        var data = JsonSerializer.SerializeToUtf8Bytes(message, _messageOpts);

        return data;
    }

    /// <summary>
    /// Deserializes JSON binary data back to a mesh message.
    /// </summary>
    /// <param name="data">The JSON binary data to deserialize.</param>
    /// <returns>The deserialized message.</returns>
    public Message DeserializeMessage(ReadOnlyMemory<byte> data)
    {
        var message = JsonSerializer.Deserialize<Message>(data.Span, _messageOpts)!;

        return message;
    }

    /// <summary>
    /// Serializes an object to binary data using the underlying JSON serializer.
    /// </summary>
    /// <param name="type">The type of the object to serialize.</param>
    /// <param name="value">The value to serialize.</param>
    /// <returns>The serialized data as binary.</returns>
    public ReadOnlyMemory<byte> SerializeData(Type type, object? value)
    {
        return _serializer.Serialize(type, value);
    }

    /// <summary>
    /// Deserializes binary data to an object using the underlying JSON serializer.
    /// </summary>
    /// <param name="type">The type to deserialize to.</param>
    /// <param name="data">The binary data to deserialize.</param>
    /// <returns>The deserialized object.</returns>
    public object? DeserializeData(Type type, ReadOnlyMemory<byte> data)
    {
        return _serializer.Deserialize(type, data);
    }
}
