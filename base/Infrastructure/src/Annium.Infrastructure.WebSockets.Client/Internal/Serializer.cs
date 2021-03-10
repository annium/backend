using System;
using System.Net.Mime;
using System.Text;
using Annium.Core.DependencyInjection;
using Annium.Infrastructure.WebSockets.Domain;
using Annium.Net.WebSockets;
using Annium.Serialization.Abstractions;

namespace Annium.Infrastructure.WebSockets.Client.Internal
{
    internal class Serializer
    {
        private static readonly string _type = MediaTypeNames.Application.Json;

        public object Instance { get; }

        public Serializer(
            IIndex<string, ISerializer<ReadOnlyMemory<byte>>> binarySerializers,
            IIndex<string, ISerializer<string>> textSerializers,
            ClientConfiguration configuration
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

        public T Deserialize<T>(SocketMessage message) => Instance switch
        {
            ISerializer<ReadOnlyMemory<byte>> x => x.Deserialize<T>(message.Data),
            ISerializer<string> x => x.Deserialize<T>(Encoding.UTF8.GetString(message.Data.Span)),
            _ => throw new NotImplementedException()
        };
    }
}