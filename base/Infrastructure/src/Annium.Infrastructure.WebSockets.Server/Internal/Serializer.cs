using System;
using System.Net.Mime;
using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;

namespace Annium.Infrastructure.WebSockets.Server.Internal
{
    internal class Serializer
    {
        private readonly ISerializer<ReadOnlyMemory<byte>> _serializer;

        public Serializer(
            IIndex<string, ISerializer<ReadOnlyMemory<byte>>> serializers
        )
        {
            _serializer = serializers[MediaTypeNames.Application.Json];
        }

        public T Deserialize<T>(ReadOnlyMemory<byte> value) =>
            _serializer.Deserialize<T>(value);

        public object Deserialize(Type type, ReadOnlyMemory<byte> value) =>
            _serializer.Deserialize(type, value);

        public ReadOnlyMemory<byte> Serialize<T>(T value) =>
            _serializer.Serialize(value);

        public ReadOnlyMemory<byte> Serialize(object value) =>
            _serializer.Serialize(value);
    }
}