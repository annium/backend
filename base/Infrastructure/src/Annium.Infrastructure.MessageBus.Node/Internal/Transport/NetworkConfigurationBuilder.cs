using System;
using Annium.Infrastructure.MessageBus.Node.Transport;
using Annium.Serialization.Abstractions;

namespace Annium.Infrastructure.MessageBus.Node.Internal.Transport
{
    internal class NetworkConfigurationBuilder : INetworkConfigurationBuilder
    {
        private EndpointsConfiguration? _endpointsConfiguration;
        private ISerializer<string>? _serializer;

        public INetworkConfigurationBuilder WithEndpoints(EndpointsConfiguration endpointsConfiguration)
        {
            _endpointsConfiguration = endpointsConfiguration;

            return this;
        }

        public INetworkConfigurationBuilder WithSerializer(ISerializer<string> serializer)
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
}