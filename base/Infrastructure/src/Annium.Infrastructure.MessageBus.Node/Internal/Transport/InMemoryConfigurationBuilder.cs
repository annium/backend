using System;
using Annium.Infrastructure.MessageBus.Node.Transport;
using Annium.Serialization.Abstractions;

namespace Annium.Infrastructure.MessageBus.Node.Internal.Transport
{
    internal class InMemoryConfigurationBuilder : IInMemoryConfigurationBuilder
    {
        private ISerializer<string>? _serializer;

        public IInMemoryConfigurationBuilder WithSerializer(ISerializer<string> serializer)
        {
            _serializer = serializer;

            return this;
        }

        public InMemoryConfiguration Build()
        {
            if (_serializer is null)
                throw new ArgumentException("MessageBus serializer is not configured");

            return new InMemoryConfiguration(_serializer);
        }
    }
}