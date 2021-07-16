using Annium.Serialization.Abstractions;

namespace Annium.Infrastructure.MessageBus.Node.Internal.Transport
{
    internal class Configuration : IConfiguration
    {
        public EndpointsConfiguration Endpoints { get; }
        public ISerializer<string> Serializer { get; }

        public Configuration(
            EndpointsConfiguration endpoints,
            ISerializer<string> serializer
        )
        {
            Endpoints = endpoints;
            Serializer = serializer;
        }
    }
}