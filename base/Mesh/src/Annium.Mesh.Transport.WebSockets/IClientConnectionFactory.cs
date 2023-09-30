using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Transport.WebSockets;

public interface IClientConnectionFactory
{
    IClientConnection Create();
}