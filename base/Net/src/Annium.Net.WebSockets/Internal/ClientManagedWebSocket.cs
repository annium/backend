using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Debug;
using NativeWebSocket = System.Net.WebSockets.ClientWebSocket;

namespace Annium.Net.WebSockets.Internal;

public class ClientManagedWebSocket : IClientManagedWebSocket, IAsyncDisposable
{
    public event Action<ReadOnlyMemory<byte>> TextReceived = delegate { };
    public event Action<ReadOnlyMemory<byte>> BinaryReceived = delegate { };

    public Task<WebSocketCloseResult> IsClosed
    {
        get
        {
            if (_wrappedListenTask is null)
                throw new InvalidOperationException("Socket is not connected");

            return _wrappedListenTask;
        }
    }

    private readonly object _locker = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private NativeWebSocket? _nativeSocket;
    private ManagedWebSocket? _managedSocket;
    private CancellationTokenSource? _listenCts;
    private Task<WebSocketCloseResult>? _listenTask;
    private Task<WebSocketCloseResult>? _wrappedListenTask;
    private bool _isDisposed;

    public async Task<bool> ConnectAsync(Uri uri, CancellationToken ct = default)
    {
        EnsureNotDisposed();

        try
        {
            await _semaphore.WaitAsync(CancellationToken.None);

            this.Trace("start");

            EnsureNotConnected();

            _nativeSocket = new NativeWebSocket();
            _managedSocket = new ManagedWebSocket(_nativeSocket);
            this.Trace($"paired with {_managedSocket.GetFullId()}");

            _managedSocket.TextReceived += OnTextReceived;
            _managedSocket.BinaryReceived += OnBinaryReceived;

            this.Trace($"connect native socket to {uri}");
            await _nativeSocket.ConnectAsync(uri, ct);

            this.Trace("create listen cts");
            _listenCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

            this.Trace("create listen task");
            _listenTask = _managedSocket.ListenAsync(_listenCts.Token);
            _wrappedListenTask = Task.Run(async () =>
            {
                this.Trace("start listen");
                var result = await _listenTask;
                await HandleClosed();

                return result;
            }, CancellationToken.None);

            return true;
        }
        catch
        {
            this.Trace("cleanup");
            // this failure can happen only in connect call, but for reliability - cleanup everything, resetting socket to not connected state
            _nativeSocket?.Dispose();
            _nativeSocket = null;
            _managedSocket = null;
            _listenCts?.Cancel();
            _listenCts?.Dispose();
            _listenCts = null;
            _listenTask = null;

            return false;
        }
        finally
        {
            this.Trace("done");

            _semaphore.Release();
        }
    }

    public async Task DisconnectAsync()
    {
        EnsureNotDisposed();

        try
        {
            await _semaphore.WaitAsync();

            this.Trace("start");

            await DisconnectPrivateAsync();
        }
        finally
        {
            this.Trace("done");

            _semaphore.Release();
        }
    }

    public async ValueTask<WebSocketSendStatus> SendTextAsync(ReadOnlyMemory<byte> text, CancellationToken ct = default)
    {
        EnsureNotDisposed();

        try
        {
            await _semaphore.WaitAsync(CancellationToken.None);

            this.Trace("start");

            if (!IsConnected())
            {
                this.Trace("closed because not connected");
                return WebSocketSendStatus.Closed;
            }

            this.Trace("send text");
            return await _managedSocket.SendTextAsync(text, ct);
        }
        finally
        {
            this.Trace("done");

            _semaphore.Release();
        }
    }

    public async ValueTask<WebSocketSendStatus> SendBinaryAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        EnsureNotDisposed();

        try
        {
            await _semaphore.WaitAsync(CancellationToken.None);

            this.Trace("start");

            if (!IsConnected())
            {
                this.Trace("closed because not connected");
                return WebSocketSendStatus.Closed;
            }

            this.Trace("send binary");
            return await _managedSocket.SendBinaryAsync(data, ct);
        }
        finally
        {
            this.Trace("done");

            _semaphore.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        lock (_locker)
        {
            if (_isDisposed)
            {
                this.Trace("already disposed");

                return;
            }

            _isDisposed = true;
        }

        try
        {
            await _semaphore.WaitAsync();

            this.Trace("marked as disposed, disconnect if connected");
            await DisconnectPrivateAsync();
        }
        finally
        {
            this.Trace("done");

            _semaphore.Release();
            _semaphore.Dispose();
        }
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

    private async Task DisconnectPrivateAsync()
    {
        this.Trace("start");

        if (!IsConnected())
        {
            this.Trace("skip - not connected");
            return;
        }

        try
        {
            _managedSocket.TextReceived -= OnTextReceived;
            _managedSocket.BinaryReceived -= OnBinaryReceived;

            this.Trace("close output");
            if (_nativeSocket.State is WebSocketState.Open or WebSocketState.CloseReceived)
                await _nativeSocket.CloseOutputAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
        }
        finally
        {
            this.Trace("cancel listen cts");
            _listenCts.Cancel();
            await _listenTask;

            this.Trace("reset socket references to null");
            _nativeSocket = null;
            _managedSocket = null;
        }

        this.Trace("done");
    }

    private async Task HandleClosed()
    {
        try
        {
            this.Trace("begin");

            await _semaphore.WaitAsync();

            this.Trace("start");

            if (!IsConnected())
            {
                this.Trace("already disconnected");
                return;
            }

            this.Trace("unsubscribe from managed socket");

            _managedSocket.TextReceived -= OnTextReceived;
            _managedSocket.BinaryReceived -= OnBinaryReceived;

            this.Trace("reset socket references to null");
            _nativeSocket = null;
            _managedSocket = null;
        }
        finally
        {
            this.Trace("done");

            _semaphore.Release();

            this.Trace("end");
        }
    }

    private void EnsureNotConnected()
    {
        // only sockets are checked, because after disconnect listen task can still be awaited
        if (_nativeSocket is not null || _managedSocket is not null)
            throw new InvalidOperationException("Socket is already connected");
    }

    [MemberNotNullWhen(true, nameof(_nativeSocket), nameof(_managedSocket), nameof(_listenCts), nameof(_listenTask), nameof(_wrappedListenTask))]
    private bool IsConnected()
    {
        return _nativeSocket is not null && _managedSocket is not null && _listenCts is not null && _listenTask is not null && _wrappedListenTask is not null;
    }

    private void EnsureNotDisposed()
    {
        lock (_locker)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("Socket is already disposed");
        }
    }
}