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
        IFinalRequestHandler<PushMessage<T>, Unit>,
        ILogSubject
    {
        public ILogger Logger { get; }
        private readonly ConnectionTracker _connectionTracker;
        private readonly Serializer _serializer;

        public PushMessageHandler(
            ConnectionTracker connectionTracker,
            Serializer serializer,
            ILogger<PushMessageHandler<T>> logger
        )
        {
            _connectionTracker = connectionTracker;
            _serializer = serializer;
            Logger = logger;
        }

        public async Task<Unit> HandleAsync(
            PushMessage<T> request,
            CancellationToken ct
        )
        {
            this.Trace($"cn {request.ConnectionId} - start");
            if (!_connectionTracker.TryGet(request.ConnectionId, out var cnRef))
            {
                this.Trace($"cn {request.ConnectionId} - not found");
                return Unit.Default;
            }

            try
            {
                if (cnRef.Value.Socket.State != WebSocketState.Open)
                {
                    this.Trace($"cn {request.ConnectionId} - socket not opened");
                    return Unit.Default;
                }

                this.Trace($"cn {request.ConnectionId} - start send");
                await cnRef.Value.Socket.SendWith(request.Message, _serializer, CancellationToken.None);
                this.Trace($"cn {request.ConnectionId} - send complete");
            }
            // socket can get closed/aborted in a moment
            catch (WebSocketException)
            {
            }
            finally
            {
                this.Trace($"cn {request.ConnectionId} - dispose ref");
                await cnRef.DisposeAsync();
            }

            return Unit.Default;
        }
    }
}