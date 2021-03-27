using System;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Primitives;
using Annium.Infrastructure.WebSockets.Domain.Models;
using Annium.Net.WebSockets;
using Microsoft.Extensions.DependencyInjection;

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
            await using var cn = new Connection(Guid.NewGuid(), socket);
            _connectionTracker.Track(cn);
            IServiceScope scope = ServiceProviderExtensions.CreateScope(_sp);
            try
            {
                await _handlerFactory.Create(scope.ServiceProvider, cn).HandleAsync();
            }
            finally
            {
                _connectionTracker.Release(cn);

                await scope.DisposeAsync();
            }
        }

        public void Shutdown()
        {
            _lifetimeManager.Stop();
        }
    }
}