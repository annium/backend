using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Infrastructure.WebSockets.Server.Internal.Handlers;

namespace Annium.Infrastructure.WebSockets.Server.Internal
{
    internal class BroadcastCoordinator : IAsyncDisposable
    {
        private readonly ConnectionTracker _connectionTracker;
        private readonly IEnumerable<IBroadcasterRunner> _runners;
        private readonly Serializer _serializer;
        private readonly CancellationTokenSource _lifetimeCts;
        private Task _runnersTask = Task.CompletedTask;

        public BroadcastCoordinator(
            ConnectionTracker connectionTracker,
            IEnumerable<IBroadcasterRunner> runners,
            Serializer serializer
        )
        {
            _connectionTracker = connectionTracker;
            _runners = runners;
            _serializer = serializer;
            _lifetimeCts = new CancellationTokenSource();
        }

        public void Start()
        {
            _runnersTask = Task.WhenAll(_runners.Select(
                x => x.Run(Broadcast, _lifetimeCts.Token)
            ));
        }

        public async ValueTask DisposeAsync()
        {
            _lifetimeCts.Cancel();
            await _runnersTask;
        }

        private void Broadcast(object message)
        {
            var connections = _connectionTracker.Slice();
            if (connections.Count == 0)
                return;
            var data = _serializer.Serialize(message);
            Task.WhenAll(connections.Select(async x => await x.Socket.Send(data, CancellationToken.None)));
        }
    }
}