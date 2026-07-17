using System;
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

        container.AddTestServerClient(x => x.WithResponseTimeout(30));
    }

    /// <summary>
    /// Cancellation token source for controlling server lifetime.
    /// </summary>
    private readonly CancellationTokenSource _serverCts;

    /// <summary>
    /// The in-memory mesh server instance.
    /// </summary>
    private readonly IServer _server;

    /// <summary>
    /// Server instance run task to await for in disposal
    /// </summary>
    private Task _serverTask = Task.CompletedTask;

    /// <summary>
    /// Initializes a new instance of the <see cref="Behavior"/> class with the specified server.
    /// </summary>
    /// <param name="server">The in-memory mesh server instance.</param>
    public Behavior(IServer server)
    {
        _serverCts = new CancellationTokenSource();
        _server = server;
    }

    /// <summary>
    /// Starts the in-memory mesh server by launching its run loop, storing the resulting task for later awaiting.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> that completes immediately after the server task is started.</returns>
    public ValueTask InitializeAsync()
    {
        _serverTask = _server.RunAsync(_serverCts.Token);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Stops the in-memory mesh server by suppressing finalization, canceling the server cancellation token, and awaiting the server run task.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> that completes when the server task has finished.</returns>
    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        await _serverCts.CancelAsync();
#pragma warning disable VSTHRD003
        await _serverTask;
#pragma warning restore VSTHRD003
    }
}
