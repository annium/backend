using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Core.Internal;
using Annium.Diagnostics.Debug;
using Annium.Net.WebSockets;

namespace Annium.Infrastructure.WebSockets.Server.Internal
{
    internal class ConnectionTracker : IAsyncDisposable
    {
        private readonly IServerLifetime _lifetime;
        public event Action<Guid> OnTrack = delegate { };
        public event Action<Guid> OnRelease = delegate { };
        private readonly ConcurrentDictionary<Guid, Connection> _connections = new();
        private readonly TaskCompletionSource<object> _disposeTcs = new();
        private bool _isDisposed;

        public ConnectionTracker(IServerLifetime lifetime)
        {
            _lifetime = lifetime;
            _lifetime.Stopping.Register(TryStop);
        }

        public Connection Track(WebSocket socket)
        {
            EnsureNotDisposed();

            var cn = new Connection(Guid.NewGuid(), socket);
            this.Trace(() => $"Track connection {cn.GetId()}");
            _connections.TryAdd(cn.Id, cn);

            this.Trace(() => $"Invoke OnTrack for connection {cn.GetId()}");
            OnTrack.Invoke(cn.Id);

            return cn;
        }

        public bool TryGet(Guid id, out Connection cn)
        {
            EnsureNotDisposed();

            return _connections.TryGetValue(id, out cn!);
        }

        public void Release(Connection cn)
        {
            EnsureNotDisposed();

            this.Trace(() => $"Try release connection {cn.GetId()}");
            if (!_connections.TryRemove(cn.Id, out _))
                return;

            this.Trace(() => $"Invoke OnRelease for connection {cn.GetId()}");
            OnRelease.Invoke(cn.Id);

            if (_lifetime.Stopping.IsCancellationRequested)
                TryStop();
        }

        public IReadOnlyCollection<Connection> Slice()
        {
            lock (_connections) return _connections.Values.ToArray();
        }

        public async ValueTask DisposeAsync()
        {
            EnsureNotDisposed();
            _isDisposed = true;

            this.Trace(() => "start");
            await _disposeTcs.Task;
            this.Trace(() => "done");
        }

        private void TryStop()
        {
            this.Trace(() => $"Unreleased connections: {_connections.Count}");
            if (_connections.IsEmpty)
                _disposeTcs.SetResult(new object());
        }

        private void EnsureNotDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(ConnectionTracker));
        }
    }
}