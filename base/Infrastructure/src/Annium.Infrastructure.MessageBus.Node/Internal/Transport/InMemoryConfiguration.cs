using Annium.Serialization.Abstractions;

namespace Annium.Infrastructure.MessageBus.Node.Internal.Transport;

internal class InMemoryConfiguration : IConfiguration
{
    public ISerializer<string> Serializer { get; }

    public InMemoryConfiguration(
        ISerializer<string> serializer
    )
    {
        Serializer = serializer;
    }
}