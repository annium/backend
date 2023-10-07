using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Transport.Abstractions;
using Annium.Net.WebSockets;

namespace Annium.Mesh.Transport.WebSockets.Internal;

internal sealed class ClientConnection : IClientConnection, ILogSubject
{
    public event Action OnConnected = delegate { };
    public event Action<ConnectionCloseStatus> OnDisconnected = delegate { };
    public event Action<Exception> OnError = delegate { };
    public event Action<ReadOnlyMemory<byte>> OnReceived = delegate { };
    public ILogger Logger { get; }
    private readonly IClientWebSocket _socket;
    private readonly Uri _uri;

    public ClientConnection(
        IClientWebSocket socket,
        Uri uri,
        ILogger logger
    )
    {
        Logger = logger;
        _socket = socket;
        _socket.OnConnected += HandleConnected;
        _socket.OnDisconnected += HandleDisconnected;
        _socket.OnError += HandleError;
        _socket.OnBinaryReceived += HandleReceived;
        _uri = uri;
    }

    public void Connect()
    {
        this.Trace("start");

        _socket.Connect(_uri);

        this.Trace("done");
    }

    public void Disconnect()
    {
        this.Trace("start");

        _socket.Disconnect();

        this.Trace("done");
    }

    public async ValueTask<ConnectionSendStatus> SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        this.Trace("start");

        var status = await _socket.SendBinaryAsync(data, ct);

        this.Trace("done");

        return ConnectionSendStatusMap.Map(status);
    }

    private void HandleConnected()
    {
        this.Trace("trigger connected");
        OnConnected();
    }

    private void HandleDisconnected(WebSocketCloseStatus status)
    {
        var mappedStatus = ConnectionCloseStatusMap.Map(status);
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