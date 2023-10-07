using System.Net.Sockets;
using System.Threading.Tasks;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Transport.Sockets;

public interface IServerConnectionFactory
{
    Task<IServerConnection> CreateAsync(Socket socket);
}