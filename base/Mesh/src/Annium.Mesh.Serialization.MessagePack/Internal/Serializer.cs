using System;
using Annium.Core.DependencyInjection;
using Annium.Mesh.Domain;
using Annium.Mesh.Serialization.Abstractions;
using Annium.Serialization.Abstractions;
using MessagePack;
using MessagePack.Resolvers;

namespace Annium.Mesh.Serialization.MessagePack.Internal;

internal class Serializer : ISerializer
{
    private static readonly MessagePackSerializerOptions MessageOpts;

    static Serializer()
    {
        var resolver = CompositeResolver.Create(
            new FormatterResolver(),
            MessagePackSerializerOptions.Standard.Resolver
        );
        MessageOpts = new MessagePackSerializerOptions(resolver);
    }

    private readonly ISerializer<ReadOnlyMemory<byte>> _serializer;

    public Serializer(IIndex<SerializerKey, ISerializer<ReadOnlyMemory<byte>>> serializers)
    {
        var key = SerializerKey.Create(Constants.SerializerKey, Annium.Serialization.MessagePack.Constants.MediaType);
        _serializer = serializers[key];
    }

    public ReadOnlyMemory<byte> SerializeMessage(Message message)
    {
        var data = MessagePackSerializer.Serialize(message, MessageOpts);

        return data;
    }

    public Message DeserializeMessage(ReadOnlyMemory<byte> data)
    {
        var message = MessagePackSerializer.Deserialize<Message>(data, MessageOpts);

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
