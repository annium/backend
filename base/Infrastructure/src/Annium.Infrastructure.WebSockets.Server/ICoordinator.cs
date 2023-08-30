using System.Threading.Tasks;
using Annium.Net.WebSockets;

namespace Annium.Infrastructure.WebSockets.Server;

public interface ICoordinator
{
    Task HandleAsync(IServerWebSocket socket);

    void Shutdown();
}