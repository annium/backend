using System;
using System.Collections.Concurrent;
using Annium.Core.Runtime.Time;
using NodaTime;

namespace Annium.Collections.Generic
{
    public class ExpiringDictionary<TKey, TValue>
        where TKey : notnull
    {
        private readonly ITimeProvider _timeProvider;
        private readonly ConcurrentDictionary<TKey, ValueTuple<TValue, Instant>> _data = new();

        public ExpiringDictionary(ITimeProvider timeProvider)
        {
            _timeProvider = timeProvider;
        }

        public void Add(TKey key, TValue value, Duration ttl)
        {
            var tuple = (value, _timeProvider.Now + ttl);
            _data.AddOrUpdate(key, tuple, (_, _) => tuple);
        }

        public bool ContainsKey(TKey key)
        {
            Cleanup();

            return _data.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            Cleanup();

            return _data.TryRemove(key, out _);
        }

        public void Clear()
        {
            _data.Clear();
        }

        private void Cleanup()
        {
            var now = _timeProvider.Now;
            var pairs = _data.ToArray();
            foreach (var (key, value) in pairs)
            {
                if (value.Item2 < now)
                    _data.TryRemove(key, out _);
            }
        }
    }
}