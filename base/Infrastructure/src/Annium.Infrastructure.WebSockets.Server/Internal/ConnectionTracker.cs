using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Infrastructure.WebSockets.Server.Internal
{
    internal class ConnectionTracker
    {
        public event Action<Guid> OnRelease = delegate { };
        private readonly ConcurrentDictionary<Guid, Connection> _connections = new();

        public void Track(Connection cn)
        {
            _connections.TryAdd(cn.Id, cn);
        }

        public bool TryGet(Guid id, out Connection cn)
        {
            return _connections.TryGetValue(id, out cn!);
        }

        public void Release(Connection cn)
        {
            if (_connections.TryRemove(cn.Id, out _))
                OnRelease.Invoke(cn.Id);
        }

        public IReadOnlyCollection<Connection> Slice()
        {
            lock (_connections) return _connections.Values.ToArray();
        }
    }
}