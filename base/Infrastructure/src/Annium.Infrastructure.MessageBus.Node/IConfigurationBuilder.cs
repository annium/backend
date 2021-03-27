using Annium.Serialization.Abstractions;

namespace Annium.Infrastructure.MessageBus.Node
{
    public interface IConfigurationBuilder
    {
        IConfigurationBuilder WithEndpoints(EndpointsConfiguration endpointsConfiguration);
        IConfigurationBuilder WithSerializer(ISerializer<string> serializer);
    }

    public interface ITestConfigurationBuilder
    {
        ITestConfigurationBuilder WithSerializer(ISerializer<string> serializer);
    }
}