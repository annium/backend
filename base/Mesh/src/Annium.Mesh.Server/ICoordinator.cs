using System.Threading.Tasks;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Server;

public interface ICoordinator
{
    Task HandleAsync(IServerConnection connection);

    void Shutdown();
}
