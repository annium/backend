using Annium.Serialization.Abstractions;

namespace Annium.Infrastructure.MessageBus.Node.Transport
{
    public interface INetworkConfigurationBuilder
    {
        INetworkConfigurationBuilder WithEndpoints(EndpointsConfiguration endpointsConfiguration);
        INetworkConfigurationBuilder WithSerializer(ISerializer<string> serializer);
    }
}