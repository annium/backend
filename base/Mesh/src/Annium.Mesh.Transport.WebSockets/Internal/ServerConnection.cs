using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Transport.Abstractions;
using Annium.Net.WebSockets;

namespace Annium.Mesh.Transport.WebSockets.Internal;

internal sealed class ServerConnection : IServerConnection, ILogSubject
{
    public ILogger Logger { get; }
    public event Action<CloseStatus> OnDisconnected = delegate { };
    public event Action<Exception> OnError = delegate { };
    public event Action<ReadOnlyMemory<byte>> OnReceived = delegate { };
    private readonly IServerWebSocket _socket;


    public ServerConnection(
        IServerWebSocket socket,
        ILogger logger
    )
    {
        Logger = logger;
        _socket = socket;
        _socket.OnDisconnected += HandleDisconnected;
        _socket.OnError += HandleError;
        _socket.OnBinaryReceived += HandleReceived;
    }

    public void Disconnect()
    {
        this.Trace("start");

        _socket.Disconnect();

        this.Trace("done");
    }

    public async ValueTask<SendStatus> SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        this.Trace("start");

        var status = await _socket.SendBinaryAsync(data, ct);

        this.Trace("done");

        return SendStatusMap.Map(status);
    }

    private void HandleDisconnected(WebSocketCloseStatus status)
    {
        var mappedStatus = CloseStatusMap.Map(status);
        this.Trace("trigger disconnected with {status}", mappedStatus);
        OnDisconnected(mappedStatus);
    }

    private void HandleError(Exception exception)
    {
        this.Trace("trigger error {exception}", exception);
        OnError(exception);
    }

    private void HandleReceived(ReadOnlyMemory<byte> data)
    {
        this.Trace("trigger received");
        OnReceived(data);
    }
}