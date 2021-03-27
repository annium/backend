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

    internal class TestConfigurationBuilder : ITestConfigurationBuilder
    {
        private ISerializer<string>? _serializer;
        private IList<string> _messageBox = new List<string>();

        public ITestConfigurationBuilder WithSerializer(ISerializer<string> serializer)
        {
            _serializer = serializer;

            return this;
        }

        public ITestConfigurationBuilder WithMessageBox(IList<string> messageBox)
        {
            _messageBox = messageBox;

            return this;
        }

        public TestConfiguration Build()
        {
            if (_serializer is null)
                throw new ArgumentException("MessageBus serializer is not configured");

            return new TestConfiguration(_serializer, _messageBox);
        }
    }
}