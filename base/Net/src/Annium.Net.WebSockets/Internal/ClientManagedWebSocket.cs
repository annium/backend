using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Debug;
using NativeWebSocket = System.Net.WebSockets.ClientWebSocket;

namespace Annium.Net.WebSockets.Internal;

public class ClientManagedWebSocket : IClientManagedWebSocket
{
    public event Action<ReadOnlyMemory<byte>> TextReceived = delegate { };
    public event Action<ReadOnlyMemory<byte>> BinaryReceived = delegate { };

    public Task<WebSocketCloseResult> IsClosed
    {
        get
        {
            if (_listenTask is null)
                throw new InvalidOperationException("Socket is not connected");

            return _listenTask;
        }
    }

    private NativeWebSocket? _nativeSocket;
    private ManagedWebSocket? _managedSocket;
    private CancellationTokenSource? _listenCts;
    private Task<WebSocketCloseResult>? _listenTask;

    public async Task<bool> ConnectAsync(Uri uri, CancellationToken ct = default)
    {
        this.TraceOld("start");

        // only sockets are checked, because after disconnect listen task can still be awaited
        if (_nativeSocket is not null || _managedSocket is not null)
            throw new InvalidOperationException("Socket is already connected");

        _nativeSocket = new NativeWebSocket();
        _managedSocket = new ManagedWebSocket(_nativeSocket);
        this.TraceOld($"paired with {_nativeSocket.GetFullId()} / {_managedSocket.GetFullId()}");

        this.TraceOld("bind events");
        _managedSocket.TextReceived += OnTextReceived;
        _managedSocket.BinaryReceived += OnBinaryReceived;

        try
        {
            this.TraceOld($"connect native socket to {uri}");
            await _nativeSocket.ConnectAsync(uri, ct);
        }
        catch (Exception e)
        {
            this.TraceOld($"failed: {e}");

            this.TraceOld("dispose native socket");
            _nativeSocket.Dispose();
            _nativeSocket = null;

            this.TraceOld("unbind events");
            _managedSocket.TextReceived -= OnTextReceived;
            _managedSocket.BinaryReceived -= OnBinaryReceived;
            _managedSocket = null;

            this.TraceOld("done (not connected)");

            return false;
        }

        this.TraceOld("create listen cts");
        _listenCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        this.TraceOld("create listen task");
        _listenTask = _managedSocket.ListenAsync(_listenCts.Token).ContinueWith(HandleClosed, CancellationToken.None);

        this.TraceOld("done (connected)");

        return true;
    }

    public async Task DisconnectAsync()
    {
        this.TraceOld("start");

        if (_nativeSocket is null || _managedSocket is null || _listenCts is null || _listenTask is null)
        {
            this.TraceOld("skip - not connected");
            return;
        }

        this.TraceOld("unbind events");
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

        this.TraceOld("cancel listen cts");
        _listenCts.Cancel();

        this.TraceOld("await listen task");
        await _listenTask;

        this.TraceOld("reset socket references to null");
        _nativeSocket = null;
        _managedSocket = null;

        this.TraceOld("done");
    }

    public ValueTask<WebSocketSendStatus> SendTextAsync(ReadOnlyMemory<byte> text, CancellationToken ct = default)
    {
        this.TraceOld("send text");

        return _managedSocket?.SendTextAsync(text, ct) ?? ValueTask.FromResult(WebSocketSendStatus.Closed);
    }

    public ValueTask<WebSocketSendStatus> SendBinaryAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        this.TraceOld("send binary");

        return _managedSocket?.SendBinaryAsync(data, ct) ?? ValueTask.FromResult(WebSocketSendStatus.Closed);
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
        this.TraceOld("start");

        if (_managedSocket is not null)
        {
            this.TraceOld("start, unsubscribe from managed socket");
            _managedSocket.TextReceived -= OnTextReceived;
            _managedSocket.BinaryReceived -= OnBinaryReceived;
        }

        this.TraceOld("reset socket references to null");
        _nativeSocket = null;
        _managedSocket = null;

        this.TraceOld("done");

        return task.Result;
    }
}