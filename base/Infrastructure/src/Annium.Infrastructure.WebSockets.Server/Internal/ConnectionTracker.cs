using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Pooling;
using Annium.Logging;
using Annium.Net.WebSockets;

namespace Annium.Infrastructure.WebSockets.Server.Internal;

internal class ConnectionTracker : IAsyncDisposable, ILogSubject
{
    public ILogger Logger { get; }
    private readonly IServerLifetime _lifetime;
    private readonly Dictionary<Guid, ConnectionRef> _connections = new();
    private readonly TaskCompletionSource<object> _disposeTcs = new();
    private bool _isDisposing;
    private bool _isDisposed;

    public ConnectionTracker(
        IServerLifetime lifetime,
        ILogger logger
    )
    {
        _lifetime = lifetime;
        Logger = logger;
        _lifetime.Stopping.Register(TryStop);
    }

    public Connection Track(IServerWebSocket socket)
    {
        EnsureNotDisposing();

        if (_lifetime.Stopping.IsCancellationRequested)
            throw new InvalidOperationException("Server is already stopping");

        var cn = new Connection(Guid.NewGuid(), socket, Logger);
        this.Trace("cn {connectionId} - start", cn.Id);
        lock (_connections)
            _connections[cn.Id] = new ConnectionRef(cn, Logger);

        this.Trace("cn {connectionId} - done", cn.Id);
        return cn;
    }

    public bool TryGet(Guid id, out ICacheReference<Connection> cn)
    {
        // not available, if already disposing
        if (_isDisposing)
        {
            this.Trace("cn {connectionId} - unavailable, is disposing", id);
            cn = null!;
            return false;
        }

        lock (_connections)
        {
            if (!_connections.TryGetValue(id, out var cnRef))
            {
                this.Trace("cn {connectionId} - missing", id);
                cn = null!;
                return false;
            }

            cnRef.Acquire();
            cn = CacheReference.Create(cnRef.Connection, () =>
            {
                cnRef.Release(_isDisposing);
                return Task.CompletedTask;
            });
        }

        return true;
    }

    public async Task Release(Guid id)
    {
        // can be called after disposing starts, but invalid, if already disposed
        EnsureNotDisposed();

        this.Trace("cn {connectionId} - start", id);
        ConnectionRef? cnRef;
        lock (_connections)
        {
            if (!_connections.Remove(id, out cnRef))
            {
                this.Trace("cn {connectionId} - not found", id);
                return;
            }
        }

        var cn = cnRef.Connection;
        cnRef.TryDispose();

        this.Trace("cn {connectionId} - wait until can be released", cn.Id);
        await cnRef.CanBeReleased;

        this.Trace("cn {connectionId} - dispose", cn.Id);

        if (_lifetime.Stopping.IsCancellationRequested)
            TryStop();
        this.Trace("cn {connectionId} - done", cn.Id);
    }

    public IReadOnlyCollection<Connection> Slice()
    {
        lock (_connections)
            return _connections.Values.Select(x => x.Connection).ToArray();
    }

    public async ValueTask DisposeAsync()
    {
        // not ensuring single call, because for some reason is invoked twice from integration tests
        _isDisposing = true;

        this.Trace("start");
        await _disposeTcs.Task;
        this.Trace("done");

        _isDisposed = true;
    }

    private void TryStop()
    {
        lock (_connections)
        {
            this.Trace("Unreleased connections: {connectionsCount}", _connections.Count);
            if (_connections.Count == 0)
                _disposeTcs.TrySetResult(new object());
        }
    }

    private void EnsureNotDisposing()
    {
        if (_isDisposing)
            throw new ObjectDisposedException(nameof(ConnectionTracker));
    }

    private void EnsureNotDisposed()
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(ConnectionTracker));
    }

    private sealed record ConnectionRef(
        Connection Connection,
        ILogger Logger
    ) : ILogSubject
    {
        public Task CanBeReleased => _disposeTcs.Task;
        private readonly TaskCompletionSource<object?> _disposeTcs = new();
        private int _refCount;

        public void Acquire()
        {
            var count = Interlocked.Increment(ref _refCount);
            this.Trace("cn {connectionId}: {count}", Connection.Id, count);
        }

        public void Release(bool tryDispose)
        {
            var count = Interlocked.Decrement(ref _refCount);
            this.Trace("cn {connectionId}: {count} ({tryDispose})", Connection.Id, count, tryDispose);
            if (count == 0 && tryDispose)
                _disposeTcs.TrySetResult(null);
        }

        public void TryDispose()
        {
            var count = Volatile.Read(ref _refCount);
            this.Trace("cn {connectionId}: {count}", Connection.Id, count);
            if (count == 0)
                _disposeTcs.TrySetResult(null);
        }
    }
}