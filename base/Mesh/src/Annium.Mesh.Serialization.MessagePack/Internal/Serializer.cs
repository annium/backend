using System;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Mesh.Domain;
using Annium.Mesh.Serialization.Abstractions;
using Annium.Serialization.Abstractions;
using MessagePack;
using MessagePack.Resolvers;

namespace Annium.Mesh.Serialization.MessagePack.Internal;

internal class Serializer : ISerializer
{
    private static readonly MessagePackSerializerOptions _messageOpts;

    static Serializer()
    {
        var resolver = CompositeResolver.Create(
            new FormatterResolver(),
            MessagePackSerializerOptions.Standard.Resolver
        );
        _messageOpts = new MessagePackSerializerOptions(resolver);
    }

    private readonly ISerializer<ReadOnlyMemory<byte>> _serializer;

    public Serializer(IServiceProvider sp)
    {
        var key = SerializerKey.Create(Constants.SerializerKey, Annium.Serialization.MessagePack.Constants.MediaType);
        _serializer = sp.ResolveKeyed<ISerializer<ReadOnlyMemory<byte>>>(key);
    }

    public ReadOnlyMemory<byte> SerializeMessage(Message message)
    {
        var data = MessagePackSerializer.Serialize(message, _messageOpts);

        return data;
    }

    public Message DeserializeMessage(ReadOnlyMemory<byte> data)
    {
        var message = MessagePackSerializer.Deserialize<Message>(data, _messageOpts);

        return message;
    }

    public ReadOnlyMemory<byte> SerializeData(Type type, object? value)
    {
        return _serializer.Serialize(type, value);
    }

    public object? DeserializeData(Type type, ReadOnlyMemory<byte> data)
    {
        return _serializer.Deserialize(type, data);
    }
}
