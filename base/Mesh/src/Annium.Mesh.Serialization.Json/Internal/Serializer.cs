using System;
using System.Text.Json;
using Annium.Core.DependencyInjection;
using Annium.Mesh.Domain;
using Annium.Mesh.Serialization.Abstractions;
using Annium.Serialization.Abstractions;

namespace Annium.Mesh.Serialization.Json.Internal;

internal class Serializer : ISerializer
{
    private static readonly JsonSerializerOptions MessageOpts;

    static Serializer()
    {
        MessageOpts = new JsonSerializerOptions();
        MessageOpts.Converters.Add(new MessageConverter());
    }

    private readonly ISerializer<ReadOnlyMemory<byte>> _serializer;

    public Serializer(IServiceProvider sp)
    {
        var key = SerializerKey.Create(Constants.SerializerKey, Annium.Serialization.Json.Constants.MediaType);
        _serializer = sp.ResolveKeyed<ISerializer<ReadOnlyMemory<byte>>>(key);
    }

    public ReadOnlyMemory<byte> SerializeMessage(Message message)
    {
        var data = JsonSerializer.SerializeToUtf8Bytes(message, MessageOpts);

        return data;
    }

    public Message DeserializeMessage(ReadOnlyMemory<byte> data)
    {
        var message = JsonSerializer.Deserialize<Message>(data.Span, MessageOpts)!;

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
