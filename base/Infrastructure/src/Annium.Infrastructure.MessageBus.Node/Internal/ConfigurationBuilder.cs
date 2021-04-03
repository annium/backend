using System;
using System.Collections.Generic;
using Annium.Serialization.Abstractions;

namespace Annium.Infrastructure.MessageBus.Node.Internal
{
    internal class ConfigurationBuilder : IConfigurationBuilder
    {
        private EndpointsConfiguration? _endpointsConfiguration;
        private ISerializer<string>? _serializer;

        public IConfigurationBuilder WithEndpoints(EndpointsConfiguration endpointsConfiguration)
        {
            _endpointsConfiguration = endpointsConfiguration;

            return this;
        }

        public IConfigurationBuilder WithSerializer(ISerializer<string> serializer)
        {
            _serializer = serializer;

            return this;
        }

        public Configuration Build()
        {
            if (_endpointsConfiguration is null)
                throw new ArgumentException("MessageBus endpoints are not configured");

            if (_serializer is null)
                throw new ArgumentException("MessageBus serializer is not configured");

            return new Configuration(_endpointsConfiguration, _serializer);
        }
    }

    internal class InMemoryConfigurationBuilder : IInMemoryConfigurationBuilder
    {
        private ISerializer<string>? _serializer;
        private IList<string>? _messageBox;

        public IInMemoryConfigurationBuilder WithSerializer(ISerializer<string> serializer)
        {
            _serializer = serializer;

            return this;
        }

        public IInMemoryConfigurationBuilder WithMessageBox(IList<string> messageBox)
        {
            _messageBox = messageBox;

            return this;
        }

        public InMemoryConfiguration Build()
        {
            if (_serializer is null)
                throw new ArgumentException("MessageBus serializer is not configured");

            return new InMemoryConfiguration(_serializer, _messageBox);
        }
    }
}