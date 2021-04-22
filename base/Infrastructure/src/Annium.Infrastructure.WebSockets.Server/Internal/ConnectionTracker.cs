using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.Internal;
using Annium.Diagnostics.Debug;

namespace Annium.Infrastructure.WebSockets.Server.Internal
{
    internal class ConnectionTracker
    {
        public event Action<Guid> OnTrack = delegate { };
        public event Action<Guid> OnRelease = delegate { };
        private readonly ConcurrentDictionary<Guid, Connection> _connections = new();

        public void Track(Connection cn)
        {
            this.Trace(() => $"Track connection {cn.GetId()}");
            _connections.TryAdd(cn.Id, cn);
            this.Trace(() => $"Invoke OnTrack for connection {cn.GetId()}");
            OnTrack.Invoke(cn.Id);
        }

        public bool TryGet(Guid id, out Connection cn)
        {
            return _connections.TryGetValue(id, out cn!);
        }

        public void Release(Connection cn)
        {
            this.Trace(() => $"Try release connection {cn.GetId()}");
            if (_connections.TryRemove(cn.Id, out _))
            {
                this.Trace(() => $"Invoke OnRelease for connection {cn.GetId()}");
                OnRelease.Invoke(cn.Id);
            }
        }

        public IReadOnlyCollection<Connection> Slice()
        {
            lock (_connections) return _connections.Values.ToArray();
        }
    }
}