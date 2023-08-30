using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Infrastructure.WebSockets.Server.Models;
using Annium.Logging;
using Annium.Net.WebSockets;

// ReSharper disable AccessToDisposedClosure

namespace Annium.Infrastructure.WebSockets.Server.Internal;

internal class Coordinator<TState> : ICoordinator, IDisposable, ILogSubject
    where TState : ConnectionStateBase
{
    public ILogger Logger { get; }
    private readonly IServiceProvider _sp;
    private readonly IServerLifetimeManager _lifetimeManager;
    private readonly ConnectionTracker _connectionTracker;
    private readonly ConnectionHandlerFactory<TState> _handlerFactory;

    public Coordinator(
        IServiceProvider sp,
        IServerLifetimeManager lifetimeManager,
        ConnectionTracker connectionTracker,
        ConnectionHandlerFactory<TState> handlerFactory,
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

    public async Task HandleAsync(IServerWebSocket socket)
    {
        using var cn = _connectionTracker.Track(socket);
        this.Trace<string>("Start for {connectionId}", cn.GetFullId());

        await using var scope = _sp.CreateAsyncScope();
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(_lifetimeManager.Stopping);

        socket.OnDisconnected += status =>
        {
            this.Trace("Notify lost {connectionId} - {status}", cn.GetFullId(), status);
            // for case, when server stops, thus cancellation occurs before connection is lost
            if (!cts.IsCancellationRequested)
                cts.Cancel();
        };

        try
        {
            await using var handler = _handlerFactory.Create(scope.ServiceProvider, cn);
            await handler.HandleAsync(cts.Token);
        }
        finally
        {
            this.Trace<string>("Release complete {connectionId}", cn.GetFullId());
            await _connectionTracker.Release(cn.Id);
        }

        this.Trace<string>("End for {connectionId}", cn.GetFullId());
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