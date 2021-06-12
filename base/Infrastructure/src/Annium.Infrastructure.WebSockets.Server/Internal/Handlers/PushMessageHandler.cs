using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Annium.Infrastructure.WebSockets.Server.Internal.Models;
using Annium.Infrastructure.WebSockets.Server.Internal.Serialization;
using Annium.Infrastructure.WebSockets.Server.Models;
using Annium.Logging.Abstractions;

namespace Annium.Infrastructure.WebSockets.Server.Internal.Handlers
{
    internal class PushMessageHandler<T> :
        IFinalRequestHandler<PushMessage<T>, Unit>
    {
        private readonly ConnectionTracker _connectionTracker;
        private readonly Serializer _serializer;
        private readonly ILogger<PushMessageHandler<T>> _logger;

        public PushMessageHandler(
            ConnectionTracker connectionTracker,
            Serializer serializer,
            ILogger<PushMessageHandler<T>> logger
        )
        {
            _connectionTracker = connectionTracker;
            _serializer = serializer;
            _logger = logger;
        }

        public async Task<Unit> HandleAsync(
            PushMessage<T> request,
            CancellationToken ct
        )
        {
            _logger.Trace($"cn {request.ConnectionId} - start");
            if (!_connectionTracker.TryGet(request.ConnectionId, out var cnRef))
            {
                _logger.Trace($"cn {request.ConnectionId} - not found");
                return Unit.Default;
            }

            try
            {
                if (cnRef.Value.Socket.State != WebSocketState.Open)
                {
                    _logger.Trace($"cn {request.ConnectionId} - socket not opened");
                    return Unit.Default;
                }

                _logger.Trace($"cn {request.ConnectionId} - start send");
                await cnRef.Value.Socket.SendWith(request.Message, _serializer, CancellationToken.None);
                _logger.Trace($"cn {request.ConnectionId} - send complete");
            }
            // socket can get closed/aborted in a moment
            catch (WebSocketException)
            {
            }
            finally
            {
                _logger.Trace($"cn {request.ConnectionId} - dispose ref");
                await cnRef.DisposeAsync();
            }

            return Unit.Default;
        }
    }
}