using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Pooling;
using Annium.Logging.Abstractions;
using Annium.Net.WebSockets;

namespace Annium.Infrastructure.WebSockets.Server.Internal
{
    internal class ConnectionTracker : IAsyncDisposable, ILogSubject
    {
        public ILogger Logger { get; }
        private readonly IServerLifetime _lifetime;
        private readonly ILoggerFactory _loggerFactory;
        private readonly Dictionary<Guid, ConnectionRef> _connections = new();
        private readonly TaskCompletionSource<object> _disposeTcs = new();
        private bool _isDisposing;
        private bool _isDisposed;

        public ConnectionTracker(
            IServerLifetime lifetime,
            ILoggerFactory loggerFactory,
            ILogger<ConnectionTracker> logger
        )
        {
            _lifetime = lifetime;
            _loggerFactory = loggerFactory;
            Logger = logger;
            _lifetime.Stopping.Register(TryStop);
        }

        public Task<Connection> Track(WebSocket socket)
        {
            EnsureNotDisposing();

            if (_lifetime.Stopping.IsCancellationRequested)
                throw new InvalidOperationException("Server is already stopping");

            var cn = new Connection(Guid.NewGuid(), socket, _loggerFactory.GetLogger<Connection>());
            this.Log().Trace($"connection {cn.Id} - start");
            lock (_connections)
                _connections[cn.Id] = new ConnectionRef(cn, Logger);

            this.Log().Trace($"connection {cn.Id} - done");
            return Task.FromResult(cn);
        }

        public bool TryGet(Guid id, out ICacheReference<Connection> cn)
        {
            // not available, if already disposing
            if (_isDisposing)
            {
                this.Log().Trace($"connection {id} - unavailable, is disposing");
                cn = null!;
                return false;
            }

            lock (_connections)
            {
                if (!_connections.TryGetValue(id, out var cnRef))
                {
                    this.Log().Trace($"connection {id} - missing");
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

            this.Log().Trace($"connection {id} - start");
            ConnectionRef cnRef;
            lock (_connections)
            {
                if (!_connections.Remove(id, out cnRef))
                {
                    this.Log().Trace($"connection {id} - not found");
                    return;
                }
            }

            var cn = cnRef.Connection;
            cnRef.TryDispose();

            this.Log().Trace($"connection {cn.Id} - wait until can be released");
            await cnRef.CanBeReleased;

            this.Log().Trace($"connection {cn.Id} - dispose");

            if (_lifetime.Stopping.IsCancellationRequested)
                TryStop();
            this.Log().Trace($"connection {cn.Id} - done");
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

            this.Log().Trace("start");
            await _disposeTcs.Task;
            this.Log().Trace("done");

            _isDisposed = true;
        }

        private void TryStop()
        {
            lock (_connections)
            {
                this.Log().Trace($"Unreleased connections: {_connections.Count}");
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

        private record ConnectionRef : ILogSubject
        {
            public ILogger Logger { get; }
            public Connection Connection { get; }
            public Task CanBeReleased => _disposeTcs.Task;
            private readonly TaskCompletionSource<object?> _disposeTcs = new();
            private int _refCount;

            public ConnectionRef(
                Connection connection,
                ILogger logger
            )
            {
                Logger = logger;
                Connection = connection;
            }

            public void Acquire()
            {
                var count = Interlocked.Increment(ref _refCount);
                this.Log().Trace($"cn {Connection.Id}: {count}");
            }

            public void Release(bool tryDispose)
            {
                var count = Interlocked.Decrement(ref _refCount);
                this.Log().Trace($"cn {Connection.Id}: {count} ({tryDispose})");
                if (count == 0 && tryDispose)
                    _disposeTcs.TrySetResult(null);
            }

            public void TryDispose()
            {
                var count = Volatile.Read(ref _refCount);
                this.Log().Trace($"cn {Connection.Id}: {count}");
                if (count == 0)
                    _disposeTcs.TrySetResult(null);
            }
        }
    }
}