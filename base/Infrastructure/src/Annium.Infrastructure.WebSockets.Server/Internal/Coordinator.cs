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
    internal class Coordinator<TState> : ICoordinator, IDisposable
        where TState : ConnectionStateBase
    {
        private readonly IServiceProvider _sp;
        private readonly IServerLifetimeManager _lifetimeManager;
        private readonly ConnectionTracker _connectionTracker;
        private readonly ConnectionHandlerFactory<TState> _handlerFactory;
        private readonly ILogger<Coordinator<TState>> _logger;

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
            _logger = logger;
            broadcastCoordinator.Start();
        }

        public async Task HandleAsync(WebSocket socket)
        {
            await using var cn = await _connectionTracker.Track(socket);
            _logger.Trace($"Start for connection {cn.GetId()}");
            await using var scope = _sp.CreateAsyncScope();
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(_lifetimeManager.Stopping);
            try
            {
                socket.ConnectionLost += () =>
                {
                    _logger.Trace($"Notify lost connection {cn.GetId()}");
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
                _logger.Trace($"Release complete connection {cn.GetId()}");
                await _connectionTracker.Release(cn.Id);
            }

            _logger.Trace($"End for connection {cn.GetId()}");
        }

        public void Shutdown()
        {
            _logger.Trace("start");
            _lifetimeManager.Stop();
        }

        public void Dispose()
        {
            _logger.Trace("start");
            Shutdown();
        }
    }
}