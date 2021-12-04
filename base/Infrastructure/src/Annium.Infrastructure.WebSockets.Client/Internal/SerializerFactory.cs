using System;
using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;
using Constants = Annium.Serialization.Json.Constants;

namespace Annium.Infrastructure.WebSockets.Client.Internal;

internal class SerializerFactory
{
    private readonly ISerializer<ReadOnlyMemory<byte>> _binarySerializer;
    private readonly ISerializer<string> _textSerializer;

    public SerializerFactory(
        IIndex<SerializerKey, ISerializer<ReadOnlyMemory<byte>>> binarySerializers,
        IIndex<SerializerKey, ISerializer<string>> textSerializers
    )
    {
        var key = SerializerKey.CreateDefault(Constants.MediaType);
        _binarySerializer = binarySerializers[key];
        _textSerializer = textSerializers[key];
    }

    public Serializer Create(IClientConfigurationBase configuration)
    {
        return new(_binarySerializer, _textSerializer, configuration);
    }
}