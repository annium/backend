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
    private readonly ReaderWriterLockSlim _locker = new();
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
        try
        {
            _locker.EnterWriteLock();

            EnsureConnectedAndNotDisposed();

            await DisconnectPrivateAsync();
        }
        finally
        {
            _locker.ExitWriteLock();
        }
    }

    public ValueTask<WebSocketSendStatus> SendTextAsync(ReadOnlyMemory<byte> text, CancellationToken ct = default)
    {
        try
        {
            _locker.EnterReadLock();

            EnsureConnectedAndNotDisposed();

            this.Trace("send text");

            return _managedSocket.SendTextAsync(text, ct);
        }
        finally
        {
            _locker.ExitReadLock();
        }
    }

    public ValueTask<WebSocketSendStatus> SendBinaryAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        try
        {
            _locker.EnterReadLock();

            EnsureConnectedAndNotDisposed();

            this.Trace("send binary");

            return _managedSocket.SendBinaryAsync(data, ct);
        }
        finally
        {
            _locker.ExitReadLock();
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            _locker.EnterWriteLock();

            if (_isDisposed)
            {
                this.Trace("already disposed");

                return;
            }

            _isDisposed = true;

            if (_isConnected)
                await DisconnectPrivateAsync();
        }
        finally
        {
            _locker.ExitWriteLock();
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

    private Task DisconnectPrivateAsync()
    {
        this.Trace("start");
        _isConnected = false;

        _managedSocket.TextReceived -= TextReceived;
        _managedSocket.BinaryReceived -= BinaryReceived;

        this.Trace("close output");

        return _nativeSocket.CloseOutputAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
    }

    private void EnsureConnectedAndNotDisposed()
    {
        if (!_isConnected)
            throw new InvalidOperationException("Socket is not connected");

        if (_isDisposed)
            throw new ObjectDisposedException("Socket is already disposed");
    }
}