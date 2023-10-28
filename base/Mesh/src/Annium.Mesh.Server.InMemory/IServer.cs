using System.Threading;
using System.Threading.Tasks;

namespace Annium.Mesh.Server.InMemory;

public interface IServer
{
    Task RunAsync(CancellationToken ct = default);
}
