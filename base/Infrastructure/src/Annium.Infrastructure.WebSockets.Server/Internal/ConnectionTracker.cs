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
        public event Func<Guid, Task> OnTrack = delegate { return Task.CompletedTask; };
        public event Func<Guid, Task> OnRelease = delegate { return Task.CompletedTask; };
        private readonly ConcurrentDictionary<Guid, Connection> _connections = new();
        private readonly TaskCompletionSource<object> _disposeTcs = new();
        private bool _isDisposing;
        private bool _isDisposed;

        public ConnectionTracker(IServerLifetime lifetime)
        {
            _lifetime = lifetime;
            _lifetime.Stopping.Register(TryStop);
        }

        public async Task<Connection> Track(WebSocket socket)
        {
            EnsureNotDisposing();

            if (_lifetime.Stopping.IsCancellationRequested)
                throw new InvalidOperationException("Server is already stopping");

            var cn = new Connection(Guid.NewGuid(), socket);
            this.Trace(() => $"connection {cn.GetId()} - start");
            _connections.TryAdd(cn.Id, cn);

            this.Trace(() => $"connection {cn.GetId()} - invoke OnTrack");
            await OnTrack.Invoke(cn.Id);

            this.Trace(() => $"connection {cn.GetId()} - done");
            return cn;
        }

        public bool TryGet(Guid id, out Connection cn)
        {
            EnsureNotDisposing();

            return _connections.TryGetValue(id, out cn!);
        }

        public async Task Release(Connection cn)
        {
            // can be called after disposing starts, but invalid, if already disposed
            EnsureNotDisposed();

            this.Trace(() => $"connection {cn.GetId()} - start");
            if (!_connections.TryRemove(cn.Id, out _))
            {
                this.Trace(() => $"connection {cn.GetId()} - not found");
                return;
            }

            this.Trace(() => $"connection {cn.GetId()} - dispose");
            await cn.DisposeAsync();

            this.Trace(() => $"connection {cn.GetId()} - invoke OnRelease");
            await OnRelease.Invoke(cn.Id);

            if (_lifetime.Stopping.IsCancellationRequested)
                TryStop();
            this.Trace(() => $"connection {cn.GetId()} - done");
        }

        public IReadOnlyCollection<Connection> Slice()
        {
            lock (_connections) return _connections.Values.ToArray();
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
            this.Trace(() => $"Unreleased connections: {_connections.Count}");
            if (_connections.IsEmpty)
                _disposeTcs.TrySetResult(new object());
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
    }
}