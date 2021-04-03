using Annium.Serialization.Abstractions;

namespace Annium.Infrastructure.MessageBus.Node
{
    public interface IConfigurationBuilder
    {
        IConfigurationBuilder WithEndpoints(EndpointsConfiguration endpointsConfiguration);
        IConfigurationBuilder WithSerializer(ISerializer<string> serializer);
    }

    public interface IInMemoryConfigurationBuilder
    {
        IInMemoryConfigurationBuilder WithSerializer(ISerializer<string> serializer);
    }
}