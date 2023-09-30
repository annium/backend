using System.Threading.Tasks;
using Annium.Net.WebSockets;

namespace Annium.Mesh.Server;

public interface ICoordinator
{
    Task HandleAsync(IServerWebSocket socket);

    void Shutdown();
}