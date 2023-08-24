using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Net.WebSockets.Internal;
using NativeWebSocket = System.Net.WebSockets.WebSocket;

namespace Annium.Net.WebSockets;

public class ServerWebSocket : IServerWebSocket
{
    public event Action<ReadOnlyMemory<byte>> TextReceived = delegate { };
    public event Action<ReadOnlyMemory<byte>> BinaryReceived = delegate { };
    public event Action<WebSocketCloseStatus> OnDisconnected = delegate { };
    public event Action<Exception> OnError = delegate { };
    private readonly IServerManagedWebSocket _managedSocket;
    private readonly IConnectionMonitor _connectionMonitor;
    private bool _isConnected = true;

    public ServerWebSocket(NativeWebSocket nativeSocket, IConnectionMonitor monitor, CancellationToken ct = default)
    {
        _managedSocket = new ServerManagedWebSocket(nativeSocket, ct);
        _managedSocket.IsClosed.ContinueWith(HandleClose, CancellationToken.None);
        _connectionMonitor = monitor;
        _connectionMonitor.Init(this);
        _connectionMonitor.Start();
        _connectionMonitor.OnConnectionLost += Disconnect;
    }

    public ServerWebSocket(NativeWebSocket nativeSocket, CancellationToken ct = default)
        : this(nativeSocket, ConnectionMonitor.None, ct)
    {
    }

    public ValueTask<WebSocketSendStatus> SendTextAsync(ReadOnlyMemory<byte> text, CancellationToken ct = default)
    {
        return _managedSocket.SendTextAsync(text, ct);
    }

    public ValueTask<WebSocketSendStatus> SendBinaryAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        return _managedSocket.SendBinaryAsync(data, ct);
    }

    public void Disconnect()
    {
        if (!_isConnected)
            return;

        _isConnected = false;
        _connectionMonitor.Stop();
        _managedSocket.DisconnectAsync();
    }

    private void HandleClose(Task<WebSocketCloseResult> task)
    {
        _isConnected = false;

        var result = task.Result;
        if (result.Exception is not null)
            OnError(result.Exception);

        OnDisconnected(result.Status);
    }
}