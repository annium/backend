using System;
using System.Net.Mime;
using System.Text;
using Annium.Core.DependencyInjection;
using Annium.Infrastructure.WebSockets.Domain;
using Annium.Serialization.Abstractions;

namespace Annium.Infrastructure.WebSockets.Server.Internal.Serialization
{
    internal class Serializer
    {
        private static readonly string _type = MediaTypeNames.Application.Json;

        public object Instance { get; }

        public Serializer(
            ServerConfiguration configuration,
            IIndex<string, ISerializer<ReadOnlyMemory<byte>>> binarySerializers,
            IIndex<string, ISerializer<string>> textSerializers
        )
        {
            Instance = configuration.Format switch
            {
                SerializationFormat.Binary => binarySerializers[_type],
                SerializationFormat.Text => textSerializers[_type],
                _ => throw new NotImplementedException(
                    $"Serialization format {configuration.Format} is not implemented"
                )
            };
        }

        public object Serialize<T>(T value) => Instance switch
        {
            ISerializer<ReadOnlyMemory<byte>> x => x.Serialize(value),
            ISerializer<string> x => x.Serialize(value),
            _ => throw new NotImplementedException()
        };

        public T Deserialize<T>(ReadOnlyMemory<byte> data) => Instance switch
        {
            ISerializer<ReadOnlyMemory<byte>> x => x.Deserialize<T>(data),
            ISerializer<string> x => x.Deserialize<T>(Encoding.UTF8.GetString(data.Span)),
            _ => throw new NotImplementedException()
        };
    }
}