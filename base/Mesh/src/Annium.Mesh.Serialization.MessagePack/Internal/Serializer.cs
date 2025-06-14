using System;
using Annium.Core.DependencyInjection;
using Annium.Mesh.Domain;
using Annium.Mesh.Serialization.Abstractions;
using Annium.Serialization.Abstractions;
using MessagePack;
using MessagePack.Resolvers;

namespace Annium.Mesh.Serialization.MessagePack.Internal;

/// <summary>
/// Provides MessagePack serialization implementation for mesh messages and data payloads.
/// </summary>
internal class Serializer : ISerializer
{
    /// <summary>
    /// MessagePack serializer options configured specifically for mesh message serialization.
    /// </summary>
    private static readonly MessagePackSerializerOptions _messageOpts;

    /// <summary>
    /// Static constructor that initializes the MessagePack serializer options with custom formatter resolver.
    /// </summary>
    static Serializer()
    {
        var resolver = CompositeResolver.Create(
            new FormatterResolver(),
            MessagePackSerializerOptions.Standard.Resolver
        );
        _messageOpts = new MessagePackSerializerOptions(resolver);
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
        var key = SerializerKey.Create(Constants.SerializerKey, Annium.Serialization.MessagePack.Constants.MediaType);
        _serializer = sp.ResolveKeyed<ISerializer<ReadOnlyMemory<byte>>>(key);
    }

    /// <summary>
    /// Serializes a mesh message to MessagePack format as binary data.
    /// </summary>
    /// <param name="message">The message to serialize.</param>
    /// <returns>The serialized message as MessagePack binary data.</returns>
    public ReadOnlyMemory<byte> SerializeMessage(Message message)
    {
        var data = MessagePackSerializer.Serialize(message, _messageOpts);

        return data;
    }

    /// <summary>
    /// Deserializes MessagePack binary data back to a mesh message.
    /// </summary>
    /// <param name="data">The MessagePack binary data to deserialize.</param>
    /// <returns>The deserialized message.</returns>
    public Message DeserializeMessage(ReadOnlyMemory<byte> data)
    {
        var message = MessagePackSerializer.Deserialize<Message>(data, _messageOpts);

        return message;
    }

    /// <summary>
    /// Serializes an object to binary data using the underlying MessagePack serializer.
    /// </summary>
    /// <param name="type">The type of the object to serialize.</param>
    /// <param name="value">The value to serialize.</param>
    /// <returns>The serialized data as binary.</returns>
    public ReadOnlyMemory<byte> SerializeData(Type type, object? value)
    {
        return _serializer.Serialize(type, value);
    }

    /// <summary>
    /// Deserializes binary data to an object using the underlying MessagePack serializer.
    /// </summary>
    /// <param name="type">The type to deserialize to.</param>
    /// <param name="data">The binary data to deserialize.</param>
    /// <returns>The deserialized object.</returns>
    public object? DeserializeData(Type type, ReadOnlyMemory<byte> data)
    {
        return _serializer.Deserialize(type, data);
    }
}
