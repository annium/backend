using Annium.Serialization.Abstractions;

namespace Annium.Infrastructure.MessageBus.Node.Transport;

public interface IInMemoryConfigurationBuilder
{
    IInMemoryConfigurationBuilder WithSerializer(ISerializer<string> serializer);
}