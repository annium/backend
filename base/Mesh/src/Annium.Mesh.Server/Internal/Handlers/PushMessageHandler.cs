using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Mediator;
using Annium.Logging;
using Annium.Mesh.Server.Internal.Models;
using Annium.Mesh.Server.Internal.Serialization;

namespace Annium.Mesh.Server.Internal.Handlers;

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
        this.Trace("cn {connectionId} - start", request.ConnectionId);
        if (!_connectionTracker.TryGet(request.ConnectionId, out var cnRef))
        {
            this.Trace("cn {connectionId} - not found", request.ConnectionId);
            return None.Default;
        }

        try
        {
            this.Trace("cn {connectionId} - start send", request.ConnectionId);
            var status = await cnRef.Value.Socket.SendBinaryAsync(_serializer.Serialize(request.Message), ct);
            this.Trace("cn {connectionId} - send status - {status}", request.ConnectionId, status);
        }
        finally
        {
            this.Trace("cn {connectionId} - dispose ref", request.ConnectionId);
            await cnRef.DisposeAsync();
        }

        return None.Default;
    }
}