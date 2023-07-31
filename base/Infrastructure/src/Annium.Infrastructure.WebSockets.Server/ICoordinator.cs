using System.Threading.Tasks;
using Annium.Net.WebSockets.Obsolete;

namespace Annium.Infrastructure.WebSockets.Server;

public interface ICoordinator
{
    Task HandleAsync(WebSocket socket);

    void Shutdown();
}