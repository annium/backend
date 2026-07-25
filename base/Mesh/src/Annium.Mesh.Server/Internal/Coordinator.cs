using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Mesh.Transport.Abstractions;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable AccessToDisposedClosure

namespace Annium.Mesh.Server.Internal;

/// <summary>
/// Core coordinator implementation that manages the lifecycle of mesh server connections.
/// </summary>
internal class Coordinator : ICoordinator, ILogSubject
{
    /// <summary>
    /// Gets the logger for this coordinator.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The service provider for creating connection-scoped services.
    /// </summary>
    private readonly IServiceProvider _sp;

    /// <summary>
    /// The server lifetime manager for controlling server shutdown.
    /// </summary>
    private readonly IServerLifetimeManager _lifetimeManager;

    /// <summary>
    /// The connection tracker for managing active connections.
    /// </summary>
    private readonly ConnectionTracker _connectionTracker;

    /// <summary>
    /// Initializes a new instance of the <see cref="Coordinator"/> class.
    /// </summary>
    /// <param name="sp">The service provider for creating connection-scoped services.</param>
    /// <param name="lifetimeManager">The server lifetime manager.</param>
    /// <param name="connectionTracker">The connection tracker for managing active connections.</param>
    /// <param name="logger">The logger for this coordinator.</param>
    public Coordinator(
        IServiceProvider sp,
        IServerLifetimeManager lifetimeManager,
        ConnectionTracker connectionTracker,
        // BroadcastCoordinator broadcastCoordinator,
        ILogger logger
    )
    {
        _sp = sp;
        _lifetimeManager = lifetimeManager;
        _connectionTracker = connectionTracker;
        Logger = logger;
        // broadcastCoordinator.Start();
    }

    /// <summary>
    /// Handles an incoming server connection by setting up its context and managing its lifecycle.
    /// </summary>
    /// <param name="connection">The server connection to handle.</param>
    /// <param name="ct">The transport/server cancellation token linked into the connection lifetime.</param>
    /// <returns>A task representing the asynchronous connection handling operation.</returns>
    public async Task HandleAsync(IServerConnection connection, CancellationToken ct)
    {
        var cid = _connectionTracker.Track(connection);
        this.Trace("Start for {id}", cid);

        await using var scope = _sp.CreateAsyncScope();
        var sp = scope.ServiceProvider;
        var ctx = sp.Resolve<ConnectionContext>();
        // Link to BOTH the server lifetime (Stopping) and the transport/server token (ct). Without ct,
        // a server shutdown that cancels its own token but hasn't yet run lifetimeManager.Stop() would
        // drain in-flight connections whose push handlers never stop — deadlocking teardown.
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(_lifetimeManager.Stopping, ct);
        ctx.Init(cid, connection, cts);

        connection.OnDisconnected += status =>
        {
            this.Trace("Notify lost {id} - {status}", cid, status);
            // for case, when server stops, thus cancellation occurs before connection is lost
            ctx.Cancel();
        };

        try
        {
            // handle connection
            var handler = sp.Resolve<ConnectionHandler>();
            await handler.HandleAsync();
        }
        finally
        {
            this.Trace("Release complete {id}", cid);
            await _connectionTracker.ReleaseAsync(cid);
            connection.Disconnect();
        }

        this.Trace("End for {id}", cid);
    }

    /// <summary>
    /// Disposes the coordinator and stops the server lifetime manager.
    /// </summary>
    public void Dispose()
    {
        this.Trace("start");
        _lifetimeManager.Stop();
        this.Trace("done");
    }
}
