using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Diagnostics.Debug;
using Annium.Infrastructure.WebSockets.Domain.Models;
using Annium.Logging.Abstractions;
using Annium.Net.WebSockets;

namespace Annium.Infrastructure.WebSockets.Server.Internal
{
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
            await using var cn = await _connectionTracker.Track(socket);
            this.Trace($"Start for connection {cn.GetId()}");
            await using var scope = _sp.CreateAsyncScope();
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(_lifetimeManager.Stopping);
            try
            {
                socket.ConnectionLost += () =>
                {
                    this.Trace($"Notify lost connection {cn.GetId()}");
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
                this.Trace($"Release complete connection {cn.GetId()}");
                await _connectionTracker.Release(cn.Id);
            }

            this.Trace($"End for connection {cn.GetId()}");
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
}