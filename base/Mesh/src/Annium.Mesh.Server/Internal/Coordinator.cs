using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Mesh.Transport.Abstractions;

// ReSharper disable AccessToDisposedClosure

namespace Annium.Mesh.Server.Internal;

internal class Coordinator : ICoordinator, IDisposable, ILogSubject
{
    public ILogger Logger { get; }
    private readonly IServiceProvider _sp;
    private readonly IServerLifetimeManager _lifetimeManager;
    private readonly ConnectionTracker _connectionTracker;
    private readonly ConnectionHandlerFactory _handlerFactory;

    public Coordinator(
        IServiceProvider sp,
        IServerLifetimeManager lifetimeManager,
        ConnectionTracker connectionTracker,
        ConnectionHandlerFactory handlerFactory,
        BroadcastCoordinator broadcastCoordinator,
        ILogger logger
    )
    {
        _sp = sp;
        _lifetimeManager = lifetimeManager;
        _connectionTracker = connectionTracker;
        _handlerFactory = handlerFactory;
        Logger = logger;
        broadcastCoordinator.Start();
    }

    public async Task HandleAsync(IServerConnection connection)
    {
        var cid = _connectionTracker.Track(connection);
        this.Trace("Start for {id}", cid);

        await using var scope = _sp.CreateAsyncScope();
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
            var handler = _handlerFactory.Create(scope.ServiceProvider, cid, connection);
            await handler.HandleAsync(cts.Token);
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