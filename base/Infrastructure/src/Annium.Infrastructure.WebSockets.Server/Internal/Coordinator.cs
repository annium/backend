using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Net.WebSockets;

namespace Annium.Infrastructure.WebSockets.Server.Internal
{
    internal class Coordinator : ICoordinator
    {
        private readonly ConnectionTracker _connectionTracker;
        private readonly ConnectionHandlerFactory _handlerFactory;
        private readonly CancellationTokenSource _cts = new();

        public Coordinator(
            ConnectionTracker connectionTracker,
            ConnectionHandlerFactory handlerFactory,
            BroadcastCoordinator broadcastCoordinator
        )
        {
            _connectionTracker = connectionTracker;
            _handlerFactory = handlerFactory;
            broadcastCoordinator.Start();
        }

        public async Task HandleAsync(WebSocket socket)
        {
            await using var cn = new Connection(Guid.NewGuid(), socket);
            _connectionTracker.Track(cn);
            await _handlerFactory.Create(cn).HandleAsync(_cts.Token);
            _connectionTracker.Release(cn);
        }

        public void Shutdown()
        {
            _cts.Cancel();
        }
    }
}