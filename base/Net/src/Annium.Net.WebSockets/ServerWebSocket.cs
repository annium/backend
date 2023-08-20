using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Debug;
using NativeWebSocket = System.Net.WebSockets.WebSocket;

namespace Annium.Net.WebSockets;

public class ServerWebSocket : IServerWebSocket
{
    public event Action<ReadOnlyMemory<byte>> TextReceived = delegate { };
    public event Action<ReadOnlyMemory<byte>> BinaryReceived = delegate { };
    public Task<WebSocketReceiveStatus> IsClosed { get; }
    private readonly NativeWebSocket _nativeSocket;
    private readonly ManagedWebSocket _managedSocket;
    private bool _isConnected = true;

    public ServerWebSocket(NativeWebSocket nativeSocket, CancellationToken ct = default)
    {
        _nativeSocket = nativeSocket;
        _managedSocket = new ManagedWebSocket(nativeSocket);
        this.Trace($"paired with {_managedSocket.GetFullId()}");

        _managedSocket.TextReceived += OnTextReceived;
        _managedSocket.BinaryReceived += OnBinaryReceived;

        this.Trace("start listen");
        IsClosed = _managedSocket.ListenAsync(ct);
    }

    public Task DisconnectAsync()
    {
        EnsureConnected();
        _isConnected = false;

        _managedSocket.TextReceived -= OnTextReceived;
        _managedSocket.BinaryReceived -= OnBinaryReceived;

        this.Trace("close output");

        return _nativeSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
    }

    public ValueTask<WebSocketSendStatus> SendTextAsync(ReadOnlyMemory<byte> text, CancellationToken ct = default)
    {
        EnsureConnected();

        this.Trace("send text");

        return _managedSocket.SendTextAsync(text, ct);
    }

    public ValueTask<WebSocketSendStatus> SendBinaryAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        EnsureConnected();

        this.Trace("send binary");

        return _managedSocket.SendBinaryAsync(data, ct);
    }

    private void OnTextReceived(ReadOnlyMemory<byte> data)
    {
        this.Trace("trigger text received");
        TextReceived(data);
    }

    private void OnBinaryReceived(ReadOnlyMemory<byte> data)
    {
        this.Trace("trigger binary received");
        BinaryReceived(data);
    }

    private void EnsureConnected()
    {
        if (!_isConnected)
            throw new InvalidOperationException("Socket is not connected");
    }
}