using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection.Container;
using Annium.Mesh.Server.InMemory;
using Annium.Mesh.Tests.System.Client;
using Annium.Mesh.Transport.InMemory;

namespace Annium.Mesh.Tests.Variants.InMemory;

public class Behavior : IBehavior
{
    public static void Register(IServiceContainer container)
    {
        container.AddMeshInMemoryTransport();
        container.AddMeshInMemoryServer();

        container.AddTestServerClient(x => x.WithResponseTimeout(6000));
    }

    private readonly IServer _server;

    public Behavior(IServer server)
    {
        _server = server;
    }

    public async Task RunServerAsync(CancellationToken ct)
    {
        await _server.RunAsync(ct);
    }
}
