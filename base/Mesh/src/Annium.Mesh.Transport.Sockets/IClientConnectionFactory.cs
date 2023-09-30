using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Transport.Sockets;

public interface IClientConnectionFactory
{
    IClientConnection Create();
}