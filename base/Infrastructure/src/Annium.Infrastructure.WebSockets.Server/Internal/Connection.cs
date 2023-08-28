using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.WebSockets.Obsolete;
using WebSocket = Annium.Net.WebSockets.Obsolete.WebSocket;

namespace Annium.Infrastructure.WebSockets.Server.Internal;

internal class Connection : IAsyncDisposable, ILogSubject
{
    public Guid Id { get; }
    public ISendingReceivingWebSocket Socket => _socket;
    public ILogger Logger { get; }
    private readonly WebSocket _socket;

    public Connection(
        Guid id,
        WebSocket socket,
        ILogger logger
    )
    {
        Id = id;
        _socket = socket;
        Logger = logger;
    }

    public async ValueTask DisposeAsync()
    {
        this.Trace("start");
        if (
            _socket.State == WebSocketState.Connecting ||
            _socket.State == WebSocketState.Open ||
            _socket.State == WebSocketState.CloseReceived
        )
        {
            this.Trace("disconnect socket");
            await _socket.DisconnectAsync();
        }

        this.Trace("dispose socket");
        await _socket.DisposeAsync();
        this.Trace("done");
    }
}