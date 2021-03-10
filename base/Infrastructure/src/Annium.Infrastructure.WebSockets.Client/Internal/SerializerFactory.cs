using System;
using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;

namespace Annium.Infrastructure.WebSockets.Client.Internal
{
    internal class SerializerFactory
    {
        private readonly IIndex<string, ISerializer<ReadOnlyMemory<byte>>> _binarySerializers;
        private readonly IIndex<string, ISerializer<string>> _textSerializers;

        public SerializerFactory(
            IIndex<string, ISerializer<ReadOnlyMemory<byte>>> binarySerializers,
            IIndex<string, ISerializer<string>> textSerializers
        )
        {
            _binarySerializers = binarySerializers;
            _textSerializers = textSerializers;
        }

        public Serializer Create(ClientConfiguration configuration)
        {
            return new Serializer(_binarySerializers, _textSerializers, configuration);
        }
    }
}