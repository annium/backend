using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Mesh.Server.InMemory;
using Annium.Mesh.Tests.EndToEnd.Base;
using Annium.Mesh.Tests.System.Client;
using Annium.Mesh.Tests.System.Client.Clients;

namespace Annium.Mesh.Tests.EndToEnd.InMemory;

public class Behavior : IBehavior
{
    public static void Register(IServiceContainer container)
    {
        container.AddMeshInMemoryTransport();
        container.AddMeshInMemoryServer();

        container.AddTestServerManagedClient<None>(x => x.WithResponseTimeout(6000));
    }

    private readonly IServer _server;
    private readonly Func<None, Task<TestServerManagedClient>> _clientFactory;

    public Behavior(
        IServer server,
        Func<None, Task<TestServerManagedClient>> clientFactory
    )
    {
        _server = server;
        _clientFactory = clientFactory;
    }

    public async Task RunServer(CancellationToken ct)
    {
        await _server.RunAsync(ct);
    }

    public async Task<TestServerManagedClient> GetClient()
    {
        var client = await _clientFactory(None.Default);

        return client;
    }
}