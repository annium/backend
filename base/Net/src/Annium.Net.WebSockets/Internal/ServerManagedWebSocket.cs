using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Debug;
using NativeWebSocket = System.Net.WebSockets.WebSocket;

namespace Annium.Net.WebSockets.Internal;

public class ServerManagedWebSocket : IServerManagedWebSocket
{
    public event Action<ReadOnlyMemory<byte>> TextReceived = delegate { };
    public event Action<ReadOnlyMemory<byte>> BinaryReceived = delegate { };
    public Task<WebSocketCloseResult> IsClosed { get; }
    private readonly NativeWebSocket _nativeSocket;
    private readonly ManagedWebSocket _managedSocket;

    public ServerManagedWebSocket(NativeWebSocket nativeSocket, CancellationToken ct = default)
    {
        _nativeSocket = nativeSocket;
        _managedSocket = new ManagedWebSocket(nativeSocket);
        this.TraceOld($"paired with {_nativeSocket.GetFullId()} / {_managedSocket.GetFullId()}");

        _managedSocket.TextReceived += OnTextReceived;
        _managedSocket.BinaryReceived += OnBinaryReceived;

        this.TraceOld("start listen");
        IsClosed = _managedSocket.ListenAsync(ct).ContinueWith(HandleClosed);
    }

    public async Task DisconnectAsync()
    {
        this.TraceOld("start");
        _managedSocket.TextReceived -= OnTextReceived;
        _managedSocket.BinaryReceived -= OnBinaryReceived;

        try
        {
            this.TraceOld("close output");
            if (_nativeSocket.State is WebSocketState.Open or WebSocketState.CloseReceived)
                await _nativeSocket.CloseOutputAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
        }
        catch (Exception e)
        {
            this.TraceOld($"failed: {e}");
        }

        this.TraceOld("done");
    }

    public ValueTask<WebSocketSendStatus> SendTextAsync(ReadOnlyMemory<byte> text, CancellationToken ct = default)
    {
        this.TraceOld("send text");

        return _managedSocket.SendTextAsync(text, ct);
    }

    public ValueTask<WebSocketSendStatus> SendBinaryAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        this.TraceOld("send binary");

        return _managedSocket.SendBinaryAsync(data, ct);
    }

    private void OnTextReceived(ReadOnlyMemory<byte> data)
    {
        this.TraceOld("trigger text received");
        TextReceived(data);
    }

    private void OnBinaryReceived(ReadOnlyMemory<byte> data)
    {
        this.TraceOld("trigger binary received");
        BinaryReceived(data);
    }

    private WebSocketCloseResult HandleClosed(Task<WebSocketCloseResult> task)
    {
        this.TraceOld("start, unsubscribe from managed socket");

        _managedSocket.TextReceived -= OnTextReceived;
        _managedSocket.BinaryReceived -= OnBinaryReceived;

        this.TraceOld("done");

        return task.Result;
    }
}