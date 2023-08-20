using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using NativeWebSocket = System.Net.WebSockets.WebSocket;

namespace Annium.Net.WebSockets;

public class ServerWebSocket : IServerWebSocket
{
    public event Action<ReadOnlyMemory<byte>> TextReceived = delegate { };
    public event Action<ReadOnlyMemory<byte>> BinaryReceived = delegate { };
    private readonly NativeWebSocket _nativeSocket;
    private readonly ManagedWebSocket _managedSocket;
    private bool _isConnected = true;

    public ServerWebSocket(NativeWebSocket nativeSocket)
    {
        _nativeSocket = nativeSocket;
        _managedSocket = new ManagedWebSocket(nativeSocket);
        _managedSocket.TextReceived += OnTextReceived;
        _managedSocket.BinaryReceived += OnBinaryReceived;
    }

    public async ValueTask DisconnectAsync()
    {
        EnsureConnected();
        _isConnected = false;

        _managedSocket.TextReceived -= OnTextReceived;
        _managedSocket.BinaryReceived -= OnBinaryReceived;

        await _nativeSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).ConfigureAwait(false);
    }

    public ValueTask<WebSocketSendStatus> SendTextAsync(ReadOnlyMemory<byte> text, CancellationToken ct = default)
    {
        EnsureConnected();

        return _managedSocket.SendTextAsync(text, ct);
    }

    public ValueTask<WebSocketSendStatus> SendBinaryAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        EnsureConnected();

        return _managedSocket.SendBinaryAsync(data, ct);
    }

    public ValueTask<WebSocketReceiveStatus> ListenAsync(CancellationToken ct)
    {
        EnsureConnected();

        return _managedSocket.ListenAsync(ct);
    }

    private void OnTextReceived(ReadOnlyMemory<byte> data) => TextReceived(data);
    private void OnBinaryReceived(ReadOnlyMemory<byte> data) => BinaryReceived(data);

    private void EnsureConnected()
    {
        if (!_isConnected)
            throw new InvalidOperationException("Socket is not connected");
    }
}