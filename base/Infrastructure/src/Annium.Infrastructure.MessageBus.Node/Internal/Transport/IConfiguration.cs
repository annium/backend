using Annium.Serialization.Abstractions;

namespace Annium.Infrastructure.MessageBus.Node.Internal.Transport;

internal interface IConfiguration
{
    ISerializer<string> Serializer { get; }
}
