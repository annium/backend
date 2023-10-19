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

    public Serializer(
        IIndex<SerializerKey, ISerializer<ReadOnlyMemory<byte>>> serializers
    )
    {
        var key = SerializerKey.Create(Constants.SerializerKey, Annium.Serialization.Json.Constants.MediaType);
        _serializer = serializers[key];
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

    public ReadOnlyMemory<byte> SerializeData<T>(T value)
    {
        return _serializer.Serialize(value);
    }

    public object? DeserializeData(ReadOnlyMemory<byte> data, Type type)
    {
        return _serializer.Deserialize(type, data);
    }
}