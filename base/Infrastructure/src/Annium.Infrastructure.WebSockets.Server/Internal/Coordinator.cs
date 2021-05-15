using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Internal;
using Annium.Diagnostics.Debug;
using Annium.Infrastructure.WebSockets.Domain.Models;
using Annium.Net.WebSockets;

namespace Annium.Infrastructure.WebSockets.Server.Internal
{
    internal class Coordinator<TState> : ICoordinator
        where TState : ConnectionStateBase
    {
        private readonly IServiceProvider _sp;
        private readonly IServerLifetimeManager _lifetimeManager;
        private readonly ConnectionTracker _connectionTracker;
        private readonly ConnectionHandlerFactory<TState> _handlerFactory;

        public Coordinator(
            IServiceProvider sp,
            IServerLifetimeManager lifetimeManager,
            ConnectionTracker connectionTracker,
            ConnectionHandlerFactory<TState> handlerFactory,
            BroadcastCoordinator broadcastCoordinator
        )
        {
            _sp = sp;
            _lifetimeManager = lifetimeManager;
            _connectionTracker = connectionTracker;
            _handlerFactory = handlerFactory;
            broadcastCoordinator.Start();
        }

        public async Task HandleAsync(WebSocket socket)
        {
            var cn = await _connectionTracker.Track(socket);
            this.Trace(() => $"Start for connection {cn.GetId()}");
            await using var scope = _sp.CreateAsyncScope();
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(_lifetimeManager.Stopping);
            try
            {
                socket.ConnectionLost += async () =>
                {
                    this.Trace(() => $"Notify lost connection {cn.GetId()}");
                    cts.Cancel();
                    this.Trace(() => $"Release lost connection {cn.GetId()}");
                    await _connectionTracker.Release(cn);
                };
                await _handlerFactory.Create(scope.ServiceProvider, cn).HandleAsync(cts.Token);
            }
            finally
            {
                if (!cts.IsCancellationRequested)
                {
                    this.Trace(() => $"Release complete connection {cn.GetId()}");
                    await _connectionTracker.Release(cn);
                }
            }

            this.Trace(() => $"End for connection {cn.GetId()}");
        }

        public void Shutdown()
        {
            _lifetimeManager.Stop();
        }
    }
}