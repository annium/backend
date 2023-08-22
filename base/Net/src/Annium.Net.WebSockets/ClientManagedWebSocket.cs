using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Debug;
using NativeWebSocket = System.Net.WebSockets.ClientWebSocket;

namespace Annium.Net.WebSockets;

public class ClientManagedWebSocket : IClientManagedWebSocket
{
    public event Action<ReadOnlyMemory<byte>> TextReceived = delegate { };
    public event Action<ReadOnlyMemory<byte>> BinaryReceived = delegate { };

    public Task<WebSocketReceiveStatus> IsClosed
    {
        get
        {
            if (_listenCts is null || _listenTask is null)
                throw new InvalidOperationException("Socket was not connected");

            return _listenTask;
        }
    }

    private NativeWebSocket? _nativeSocket;
    private ManagedWebSocket? _managedSocket;
    private CancellationTokenSource? _listenCts;
    private Task<WebSocketReceiveStatus>? _listenTask;

    public async Task ConnectAsync(Uri uri, CancellationToken ct = default)
    {
        EnsureNotConnected();

        var nativeSocket = new NativeWebSocket();
        _nativeSocket = nativeSocket;
        _managedSocket = new ManagedWebSocket(nativeSocket);
        this.Trace($"paired with {_managedSocket.GetFullId()}");

        _managedSocket.TextReceived += OnTextReceived;
        _managedSocket.BinaryReceived += OnBinaryReceived;

        this.Trace("connect native socket");
        await nativeSocket.ConnectAsync(uri, ct);

        this.Trace("start listen");
        _listenCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _listenTask = _managedSocket.ListenAsync(_listenCts.Token);
    }

    public async Task DisconnectAsync()
    {
        EnsureConnected();

        _managedSocket.TextReceived -= OnTextReceived;
        _managedSocket.BinaryReceived -= OnBinaryReceived;

        this.Trace("close output");
        await _nativeSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);

        this.Trace("cancel listen cts");
        _listenCts.Cancel();
        await _listenTask;

        this.Trace("reset socket references to null");
        _nativeSocket = null;
        _managedSocket = null;
    }

    public ValueTask<WebSocketSendStatus> SendTextAsync(ReadOnlyMemory<byte> text, CancellationToken ct = default)
    {
        if (!IsConnected())
        {
            this.Trace("closed because not connected");
            return ValueTask.FromResult(WebSocketSendStatus.Closed);
        }

        this.Trace("send text");
        return _managedSocket.SendTextAsync(text, ct);
    }

    public ValueTask<WebSocketSendStatus> SendBinaryAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        if (!IsConnected())
        {
            this.Trace("closed because not connected");
            return ValueTask.FromResult(WebSocketSendStatus.Closed);
        }

        this.Trace("send binary");
        return _managedSocket.SendBinaryAsync(data, ct);
    }

    private void OnTextReceived(ReadOnlyMemory<byte> data) => TextReceived(data);
    private void OnBinaryReceived(ReadOnlyMemory<byte> data) => BinaryReceived(data);

    private void EnsureNotConnected()
    {
        // only sockets are checked, because after disconnect listen task can still be awaited
        if (_nativeSocket is not null || _managedSocket is not null)
            throw new InvalidOperationException("Socket is already connected");
    }

    [MemberNotNull(nameof(_nativeSocket), nameof(_managedSocket), nameof(_listenCts), nameof(_listenTask))]
    private void EnsureConnected()
    {
        if (_nativeSocket is null || _managedSocket is null || _listenCts is null || _listenTask is null)
            throw new InvalidOperationException("Socket is not connected");
    }

    [MemberNotNullWhen(true, nameof(_nativeSocket), nameof(_managedSocket), nameof(_listenCts), nameof(_listenTask))]
    private bool IsConnected()
    {
        return _nativeSocket is not null && _managedSocket is not null && _listenCts is not null && _listenTask is not null;
    }
}