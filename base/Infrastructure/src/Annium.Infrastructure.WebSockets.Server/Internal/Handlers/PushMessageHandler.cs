using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Annium.Infrastructure.WebSockets.Server.Internal.Models;
using Annium.Infrastructure.WebSockets.Server.Internal.Serialization;
using Annium.Logging;

namespace Annium.Infrastructure.WebSockets.Server.Internal.Handlers;

internal class PushMessageHandler<T> :
    IFinalRequestHandler<PushMessage<T>, None>,
    ILogSubject
{
    public ILogger Logger { get; }
    private readonly ConnectionTracker _connectionTracker;
    private readonly Serializer _serializer;

    public PushMessageHandler(
        ConnectionTracker connectionTracker,
        Serializer serializer,
        ILogger logger
    )
    {
        _connectionTracker = connectionTracker;
        _serializer = serializer;
        Logger = logger;
    }

    public async Task<None> HandleAsync(
        PushMessage<T> request,
        CancellationToken ct
    )
    {
        this.Trace($"cn {request.ConnectionId} - start");
        if (!_connectionTracker.TryGet(request.ConnectionId, out var cnRef))
        {
            this.Trace($"cn {request.ConnectionId} - not found");
            return None.Default;
        }

        try
        {
            if (cnRef.Value.Socket.State != WebSocketState.Open)
            {
                this.Trace($"cn {request.ConnectionId} - socket not opened");
                return None.Default;
            }

            this.Trace($"cn {request.ConnectionId} - start send");
            await cnRef.Value.Socket.SendWith(request.Message, _serializer, CancellationToken.None);
            this.Trace($"cn {request.ConnectionId} - send complete");
        }
        // socket can get closed/aborted in a moment
        catch (WebSocketException e)
        {
            this.Trace($"cn {request.ConnectionId} - send failed due to socket exception: {e}");
        }
        finally
        {
            this.Trace($"cn {request.ConnectionId} - dispose ref");
            await cnRef.DisposeAsync();
        }

        return None.Default;
    }
}