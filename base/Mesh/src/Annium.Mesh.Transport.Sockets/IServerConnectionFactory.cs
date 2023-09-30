using System.Net.Sockets;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Transport.Sockets;

public interface IServerConnectionFactory
{
    IServerConnection Create(Socket socket);
}