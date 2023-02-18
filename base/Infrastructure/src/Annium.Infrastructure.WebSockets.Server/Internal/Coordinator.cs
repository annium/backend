using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Debug;
using Annium.Infrastructure.WebSockets.Server.Models;
using Annium.Logging.Abstractions;
using Annium.Net.WebSockets;

// ReSharper disable AccessToDisposedClosure

namespace Annium.Infrastructure.WebSockets.Server.Internal;

internal class Coordinator<TState> : ICoordinator, IDisposable, ILogSubject<Coordinator<TState>>
    where TState : ConnectionStateBase
{
    public ILogger<Coordinator<TState>> Logger { get; }
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
        ILogger<Coordinator<TState>> logger
    )
    {
        _sp = sp;
        _lifetimeManager = lifetimeManager;
        _connectionTracker = connectionTracker;
        _handlerFactory = handlerFactory;
        Logger = logger;
        broadcastCoordinator.Start();
    }

    public async Task HandleAsync(WebSocket socket)
    {
        await using var cn = _connectionTracker.Track(socket);
        this.Log().Trace($"Start for connection {cn.GetId()}");
        await using var scope = _sp.CreateAsyncScope();
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(_lifetimeManager.Stopping);
        try
        {
            socket.ConnectionLost += () =>
            {
                this.Log().Trace($"Notify lost connection {cn.GetId()}");
                // for case, when server stops, thus cancellation occurs before connection is lost
                if (!cts.IsCancellationRequested)
                    cts.Cancel();

                return Task.CompletedTask;
            };
            await using var handler = _handlerFactory.Create(scope.ServiceProvider, cn);
            await handler.HandleAsync(cts.Token);
        }
        finally
        {
            this.Log().Trace($"Release complete connection {cn.GetId()}");
            await _connectionTracker.Release(cn.Id);
        }

        this.Log().Trace($"End for connection {cn.GetId()}");
    }

    public void Shutdown()
    {
        this.Log().Trace("start");
        _lifetimeManager.Stop();
    }

    public void Dispose()
    {
        this.Log().Trace("start");
        Shutdown();
    }
}