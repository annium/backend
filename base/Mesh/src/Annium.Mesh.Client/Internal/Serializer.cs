using System;
using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;
using Constants = Annium.Serialization.Json.Constants;

namespace Annium.Mesh.Client.Internal;

internal class Serializer
{
    private readonly ISerializer<ReadOnlyMemory<byte>> _serializer;

    public Serializer(
        IIndex<SerializerKey, ISerializer<ReadOnlyMemory<byte>>> binarySerializers
    )
    {
        var key = SerializerKey.CreateDefault(Constants.MediaType);
        _serializer = binarySerializers[key];
    }

    public ReadOnlyMemory<byte> Serialize<T>(T value) => _serializer.Serialize(value);

    public T Deserialize<T>(ReadOnlyMemory<byte> data) => _serializer.Deserialize<T>(data);
}