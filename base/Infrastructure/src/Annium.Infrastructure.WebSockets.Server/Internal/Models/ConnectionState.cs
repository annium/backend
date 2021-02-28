using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Annium.Core.Primitives;
using Annium.Infrastructure.WebSockets.Domain.Models;

namespace Annium.Infrastructure.WebSockets.Server.Internal.Models
{
    internal class ConnectionState : IConnectionState
    {
        public Guid ConnectionId { get; }
        private ManualResetEventSlim _gate = new(true);
        private readonly ConcurrentDictionary<string, object> _data = new();

        public ConnectionState(Guid connectionId)
        {
            ConnectionId = connectionId;
        }

        public IDisposable Lock()
        {
            _gate.Wait();
            return Disposable.Create(_gate.Set);
        }

        public bool Contains(string key) => _data.ContainsKey(key);

        public bool TryGet<T>(string key, out T value)
            where T : notnull
        {
            var result = _data.TryGetValue(key, out var raw);
            if (!result)
            {
                value = default!;
                return false;
            }

            if (raw is not T val)
                throw new InvalidCastException($"Can't cast connection state item of type {raw?.GetType().FriendlyName()} to {typeof(T).FriendlyName()}");

            value = val;
            return true;
        }

        public T Get<T>(string key)
            where T : notnull
        {
            var result = _data.TryGetValue(key, out var raw);
            if (!result)
                throw new KeyNotFoundException($"Connection state doesn't contain '{key}' item");

            if (raw is not T val)
                throw new InvalidCastException($"Can't cast connection state item of type {raw?.GetType().FriendlyName()} to {typeof(T).FriendlyName()}");

            return val;
        }

        public void Set<T>(string key, T value)
            where T : notnull
        {
            _data.AddOrUpdate(key, value, (_, _) => value);
        }
    }
}