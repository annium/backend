using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Debug;
using NativeWebSocket = System.Net.WebSockets.WebSocket;

namespace Annium.Net.WebSockets.Internal;

public class ServerManagedWebSocket : IServerManagedWebSocket, IAsyncDisposable
{
    public event Action<ReadOnlyMemory<byte>> TextReceived = delegate { };
    public event Action<ReadOnlyMemory<byte>> BinaryReceived = delegate { };
    public Task<WebSocketCloseResult> IsClosed { get; }
    private readonly object _locker = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly NativeWebSocket _nativeSocket;
    private readonly ManagedWebSocket _managedSocket;
    private bool _isDisposed;
    private bool _isConnected = true;

    public ServerManagedWebSocket(NativeWebSocket nativeSocket, CancellationToken ct = default)
    {
        _nativeSocket = nativeSocket;
        _managedSocket = new ManagedWebSocket(nativeSocket);
        this.Trace($"paired with {_managedSocket.GetFullId()}");

        _managedSocket.TextReceived += OnTextReceived;
        _managedSocket.BinaryReceived += OnBinaryReceived;

        this.Trace("start listen");
        IsClosed = _managedSocket.ListenAsync(ct);
    }

    public async Task DisconnectAsync()
    {
        EnsureNotDisposed();

        try
        {
            await _semaphore.WaitAsync();

            this.Trace("start");

            EnsureConnected();

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

            EnsureConnected();

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

            EnsureConnected();

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

            this.Trace("start");

            if (_isConnected)
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

        _isConnected = false;

        _managedSocket.TextReceived -= TextReceived;
        _managedSocket.BinaryReceived -= BinaryReceived;

        this.Trace("close output");

        await _nativeSocket.CloseOutputAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);

        this.Trace("done");
    }

    private void EnsureConnected()
    {
        if (!_isConnected)
            throw new InvalidOperationException("Socket is not connected");
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