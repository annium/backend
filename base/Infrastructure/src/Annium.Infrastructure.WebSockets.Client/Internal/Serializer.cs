using System;
using System.Text;
using Annium.Infrastructure.WebSockets.Domain;
using Annium.Net.WebSockets;
using Annium.Serialization.Abstractions;

namespace Annium.Infrastructure.WebSockets.Client.Internal
{
    internal class Serializer
    {
        public object Instance { get; }

        public Serializer(
            ISerializer<ReadOnlyMemory<byte>> binarySerializer,
            ISerializer<string> textSerializer,
            ClientConfiguration configuration
        )
        {
            Instance = configuration.Format switch
            {
                SerializationFormat.Binary => binarySerializer,
                SerializationFormat.Text   => textSerializer,
                _ => throw new NotImplementedException(
                    $"Serialization format {configuration.Format} is not implemented"
                )
            };
        }

        public T Deserialize<T>(SocketMessage message) => Instance switch
        {
            ISerializer<ReadOnlyMemory<byte>> x => x.Deserialize<T>(message.Data),
            ISerializer<string> x               => x.Deserialize<T>(Encoding.UTF8.GetString(message.Data.Span)),
            _                                   => throw new NotImplementedException()
        };
    }
}