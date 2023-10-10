using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Server.Internal;

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

    public void Track(IServerConnection connection)
    {
        this.Trace("cn {connectionId} - start", connection.Id);

        EnsureNotDisposing();

        if (_lifetime.Stopping.IsCancellationRequested)
            throw new InvalidOperationException("Server is already stopping");

        lock (_connections)
            _connections[connection.Id] = new ConnectionRef(connection, Logger);

        this.Trace("cn {connectionId} - done", connection.Id);
    }

    public bool TryGet(
        Guid id,
        [NotNullWhen(true)] out IDisposableReference<ISendingReceivingConnection>? connectionRef
    )
    {
        // not available, if already disposing
        if (_isDisposing)
        {
            this.Trace("cn {connectionId} - unavailable, is disposing", id);
            connectionRef = null;
            return false;
        }

        lock (_connections)
        {
            if (!_connections.TryGetValue(id, out var cnRef))
            {
                this.Trace("cn {connectionId} - missing", id);
                connectionRef = null;
                return false;
            }

            cnRef.Acquire();
            connectionRef = Disposable.Reference(cnRef.Connection, () =>
            {
                cnRef.Release(_isDisposing);
                return Task.CompletedTask;
            });

            return true;
        }
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

        var connection = cnRef.Connection;
        this.Trace("cn {connectionId} - try dispose", connection.Id);
        cnRef.TryDispose();

        this.Trace("cn {connectionId} - wait until can be released", connection.Id);
        await cnRef.CanBeReleased;

        if (_lifetime.Stopping.IsCancellationRequested)
        {
            this.Trace("cn {connectionId} - try stop", connection.Id);
            TryStop();
        }

        this.Trace("cn {connectionId} - done", connection.Id);
    }

    public IReadOnlyCollection<ISendingConnection> GetSendingConnections()
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
        IServerConnection Connection,
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