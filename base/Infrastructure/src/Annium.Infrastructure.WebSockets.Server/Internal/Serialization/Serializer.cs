using System;
using System.Text;
using Annium.Core.DependencyInjection;
using Annium.Infrastructure.WebSockets.Domain;
using Annium.Serialization.Abstractions;
using Constants = Annium.Serialization.Json.Constants;

namespace Annium.Infrastructure.WebSockets.Server.Internal.Serialization;

internal class Serializer
{
    public object Instance { get; }

    public Serializer(
        ServerConfiguration configuration,
        IIndex<SerializerKey, ISerializer<ReadOnlyMemory<byte>>> binarySerializers,
        IIndex<SerializerKey, ISerializer<string>> textSerializers
    )
    {
        var key = SerializerKey.CreateDefault(Constants.MediaType);
        Instance = configuration.Format switch
        {
            SerializationFormat.Binary => binarySerializers[key],
            SerializationFormat.Text   => textSerializers[key],
            _ => throw new NotImplementedException(
                $"Serialization format {configuration.Format} is not implemented"
            )
        };
    }

    public object Serialize<T>(T value) => Instance switch
    {
        ISerializer<ReadOnlyMemory<byte>> x => x.Serialize(value),
        ISerializer<string> x               => x.Serialize(value),
        _                                   => throw new NotImplementedException()
    };

    public T Deserialize<T>(ReadOnlyMemory<byte> data) => Instance switch
    {
        ISerializer<ReadOnlyMemory<byte>> x => x.Deserialize<T>(data),
        ISerializer<string> x               => x.Deserialize<T>(Encoding.UTF8.GetString(data.Span)),
        _                                   => throw new NotImplementedException()
    };
}