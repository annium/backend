using System;
using Annium.Core.DependencyInjection;
using Annium.Mesh.Serialization.Abstractions;
using Annium.Serialization.Abstractions;

namespace Annium.Mesh.Serialization.Json.Internal;

internal class Serializer : ISerializer
{
    private readonly ISerializer<ReadOnlyMemory<byte>> _serializer;

    public Serializer(
        IIndex<SerializerKey, ISerializer<ReadOnlyMemory<byte>>> serializers
    )
    {
        var key = SerializerKey.Create(Constants.SerializerKey, Annium.Serialization.Json.Constants.MediaType);
        _serializer = serializers[key];
    }

    public ReadOnlyMemory<byte> Serialize<T>(T value) => _serializer.Serialize(value);

    public T Deserialize<T>(ReadOnlyMemory<byte> data) => _serializer.Deserialize<T>(data);
}