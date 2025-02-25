using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Client;

public interface IClientFactory
{
    IClient Create(IClientConfiguration configuration);
    IManagedClient Create(IManagedConnection connection, IClientConfiguration configuration);
}
