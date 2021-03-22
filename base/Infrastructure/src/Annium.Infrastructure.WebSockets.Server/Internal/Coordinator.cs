using System;
using System.Threading.Tasks;
using Annium.Net.WebSockets;

namespace Annium.Infrastructure.WebSockets.Server.Internal
{
    internal class Coordinator : ICoordinator
    {
        private readonly IServerLifetimeManager _lifetimeManager;
        private readonly ConnectionTracker _connectionTracker;
        private readonly ConnectionHandlerFactory _handlerFactory;

        public Coordinator(
            IServerLifetimeManager lifetimeManager,
            ConnectionTracker connectionTracker,
            ConnectionHandlerFactory handlerFactory,
            BroadcastCoordinator broadcastCoordinator
        )
        {
            _lifetimeManager = lifetimeManager;
            _connectionTracker = connectionTracker;
            _handlerFactory = handlerFactory;
            broadcastCoordinator.Start();
        }

        public async Task HandleAsync(WebSocket socket)
        {
            await using var cn = new Connection(Guid.NewGuid(), socket);
            _connectionTracker.Track(cn);
            try
            {
                await _handlerFactory.Create(cn).HandleAsync();
            }
            finally
            {
                _connectionTracker.Release(cn);
            }
        }

        public void Shutdown()
        {
            _lifetimeManager.Stop();
        }
    }
}