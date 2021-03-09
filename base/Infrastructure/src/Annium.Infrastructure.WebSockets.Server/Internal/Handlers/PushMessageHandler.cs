using System;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Annium.Infrastructure.WebSockets.Server.Internal.Models;
using Annium.Infrastructure.WebSockets.Server.Models;

namespace Annium.Infrastructure.WebSockets.Server.Internal.Handlers
{
    internal class PushMessageHandler<T> :
        IFinalRequestHandler<PushMessage<T>, Unit>
    {
        private readonly IServerLifetime _lifetime;
        private readonly ConnectionTracker _connectionTracker;
        private readonly Serializer _serializer;

        public PushMessageHandler(
            IServerLifetime lifetime,
            ConnectionTracker connectionTracker,
            Serializer serializer
        )
        {
            _lifetime = lifetime;
            _connectionTracker = connectionTracker;
            _serializer = serializer;
        }

        public async Task<Unit> HandleAsync(
            PushMessage<T> request,
            CancellationToken ct
        )
        {
            if (_lifetime.Stopping.IsCancellationRequested)
                return Unit.Default;

            if (!_connectionTracker.TryGet(request.ConnectionId, out var cn))
                throw new InvalidOperationException($"Failed to find connection {request.ConnectionId}");

            if (cn.Socket.State == WebSocketState.Open)
            {
                try
                {
                    await cn.Socket.Send(_serializer.Serialize(request.Message), CancellationToken.None);
                }
                // socket can get closed/aborted in a moment
                catch (WebSocketException)
                {
                }
            }

            return Unit.Default;
        }
    }
}