using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Mesh.Server.InMemory;
using Annium.Mesh.Tests.System.Client;
using Annium.Mesh.Transport.InMemory;

namespace Annium.Mesh.Tests.Variants.InMemory;

/// <summary>
/// Test behavior implementation for in-memory mesh transport, configuring and running an in-memory server.
/// </summary>
public class Behavior : IBehavior
{
    /// <summary>
    /// Registers services required for in-memory mesh transport and server functionality.
    /// </summary>
    /// <param name="container">The service container to register services in.</param>
    public static void Register(IServiceContainer container)
    {
        container.AddMeshInMemoryTransport();
        container.AddMeshInMemoryServer();

        container.AddTestServerClient(x => x.WithResponseTimeout(6000));
    }

    /// <summary>
    /// The in-memory mesh server instance.
    /// </summary>
    private readonly IServer _server;

    /// <summary>
    /// Initializes a new instance of the <see cref="Behavior"/> class with the specified server.
    /// </summary>
    /// <param name="server">The in-memory mesh server instance.</param>
    public Behavior(IServer server)
    {
        _server = server;
    }

    /// <summary>
    /// Runs the in-memory mesh server asynchronously for the duration of the test.
    /// </summary>
    /// <param name="ct">The cancellation token to stop the server.</param>
    /// <returns>A task representing the asynchronous server operation.</returns>
    public async Task RunServerAsync(CancellationToken ct)
    {
        await _server.RunAsync(ct);
    }
}
