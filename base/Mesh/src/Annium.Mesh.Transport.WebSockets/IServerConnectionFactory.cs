using System.Net.WebSockets;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Transport.WebSockets;

public interface IServerConnectionFactory
{
    IServerConnection Create(WebSocket socket);
}