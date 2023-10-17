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
        _connectionTracker.Track(connection);
        this.Trace("Start for {connectionId}", connection.Id);

        await using var scope = _sp.CreateAsyncScope();
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(_lifetimeManager.Stopping);

        connection.OnDisconnected += status =>
        {
            this.Trace("Notify lost {connectionId} - {status}", connection.Id, status);
            // for case, when server stops, thus cancellation occurs before connection is lost
            if (!cts.IsCancellationRequested)
                cts.Cancel();
        };

        try
        {
            var handler = _handlerFactory.Create(scope.ServiceProvider, connection);
            await handler.HandleAsync(cts.Token);
        }
        finally
        {
            this.Trace("Release complete {connectionId}", connection.Id);
            await _connectionTracker.Release(connection.Id);
            connection.Disconnect();
        }

        this.Trace("End for {connectionId}", connection.Id);
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