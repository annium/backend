using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Internal;
using Annium.Extensions.Pooling;
using Annium.Net.WebSockets;

namespace Annium.Infrastructure.WebSockets.Server.Internal
{
    internal class ConnectionTracker : IAsyncDisposable
    {
        private readonly IServerLifetime _lifetime;
        private readonly Dictionary<Guid, ConnectionRef> _connections = new();
        private readonly TaskCompletionSource<object> _disposeTcs = new();
        private bool _isDisposing;
        private bool _isDisposed;

        public ConnectionTracker(IServerLifetime lifetime)
        {
            _lifetime = lifetime;
            _lifetime.Stopping.Register(TryStop);
        }

        public Task<Connection> Track(WebSocket socket)
        {
            EnsureNotDisposing();

            if (_lifetime.Stopping.IsCancellationRequested)
                throw new InvalidOperationException("Server is already stopping");

            var cn = new Connection(Guid.NewGuid(), socket);
            this.Trace(() => $"connection {cn.Id} - start");
            lock (_connections)
                _connections[cn.Id] = new ConnectionRef(cn);

            this.Trace(() => $"connection {cn.Id} - done");
            return Task.FromResult(cn);
        }

        public bool TryGet(Guid id, out ICacheReference<Connection> cn)
        {
            // not available, if already disposing
            if (_isDisposing)
            {
                this.Trace(() => $"connection {id} - unavailable, is disposing");
                cn = null!;
                return false;
            }

            lock (_connections)
            {
                if (!_connections.TryGetValue(id, out var cnRef))
                {
                    this.Trace(() => $"connection {id} - missing");
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

            this.Trace(() => $"connection {id} - start");
            ConnectionRef cnRef;
            lock (_connections)
            {
                if (!_connections.Remove(id, out cnRef))
                {
                    this.Trace(() => $"connection {id} - not found");
                    return;
                }
            }

            var cn = cnRef.Connection;
            cnRef.TryDispose();

            this.Trace(() => $"connection {cn.Id} - wait until can be released");
            await cnRef.CanBeReleased;

            this.Trace(() => $"connection {cn.Id} - dispose");

            if (_lifetime.Stopping.IsCancellationRequested)
                TryStop();
            this.Trace(() => $"connection {cn.Id} - done");
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

            this.Trace(() => "start");
            await _disposeTcs.Task;
            this.Trace(() => "done");

            _isDisposed = true;
        }

        private void TryStop()
        {
            lock (_connections)
            {
                this.Trace(() => $"Unreleased connections: {_connections.Count}");
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

        private record ConnectionRef
        {
            public Connection Connection { get; }
            public Task CanBeReleased => _disposeTcs.Task;
            private readonly TaskCompletionSource<object?> _disposeTcs = new();
            private int _refCount;

            public ConnectionRef(Connection connection)
            {
                Connection = connection;
            }

            public void Acquire()
            {
                var count = Interlocked.Increment(ref _refCount);
                this.Trace(() => $"cn {Connection.Id}: {count}");
            }

            public void Release(bool tryDispose)
            {
                var count = Interlocked.Decrement(ref _refCount);
                this.Trace(() => $"cn {Connection.Id}: {count} ({tryDispose})");
                if (count == 0 && tryDispose)
                    _disposeTcs.TrySetResult(null);
            }

            public void TryDispose()
            {
                var count = Volatile.Read(ref _refCount);
                this.Trace(() => $"cn {Connection.Id}: {count}");
                if (count == 0)
                    _disposeTcs.TrySetResult(null);
            }
        }
    }
}