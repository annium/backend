using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Mesh.Server.Internal.Models;
using Annium.Mesh.Transport.Abstractions;

// ReSharper disable AccessToDisposedClosure

namespace Annium.Mesh.Server.Internal;

internal class Coordinator : ICoordinator, IDisposable, ILogSubject
{
    public ILogger Logger { get; }
    private readonly IServiceProvider _sp;
    private readonly IServerLifetimeManager _lifetimeManager;
    private readonly ConnectionTracker _connectionTracker;

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

    public async Task HandleAsync(IServerConnection connection)
    {
        var cid = _connectionTracker.Track(connection);
        this.Trace("Start for {id}", cid);

        await using var scope = _sp.CreateAsyncScope();
        var sp = scope.ServiceProvider;
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(_lifetimeManager.Stopping);

        connection.OnDisconnected += status =>
        {
            this.Trace("Notify lost {id} - {status}", cid, status);
            // for case, when server stops, thus cancellation occurs before connection is lost
            if (!cts.IsCancellationRequested)
                cts.Cancel();
        };

        try
        {
            // handle connection
            var ctx = sp.Resolve<ConnectionContext>();
            ctx.Init(cid, connection, cts);
            await using var handler = sp.Resolve<ConnectionHandler>();
            await handler.HandleAsync();
        }
        finally
        {
            this.Trace("Release complete {id}", cid);
            await _connectionTracker.Release(cid);
            connection.Disconnect();
        }

        this.Trace("End for {id}", cid);
    }

    public void Shutdown()
    {
        this.Trace("start");
        _lifetimeManager.Stop();
    }

    public void Dispose()
    {
        this.Trace("start");
        Shutdown();
    }
}
