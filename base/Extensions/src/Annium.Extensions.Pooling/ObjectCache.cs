using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Annium.Extensions.Pooling
{
    public class ObjectCache<TKey, TValue> : IObjectCache<TKey, TValue>, ILogSubject
        where TKey : notnull
        where TValue : notnull
    {
        public ILogger Logger { get; }
        private readonly IDictionary<TKey, CacheEntry> _entries = new Dictionary<TKey, CacheEntry>();
        private readonly Func<TKey, Task<TValue>>? _factory;
        private readonly Func<TKey, Task<ICacheReference<TValue>>>? _externalFactory;
        private readonly Func<TValue, Task> _suspend;
        private readonly Func<TValue, Task> _resume;

        public ObjectCache(
            Func<TKey, Task<TValue>> factory,
            ILogger<ObjectCache<TKey, TValue>> logger
        ) : this(
            factory,
            _ => Task.CompletedTask,
            _ => Task.CompletedTask,
            logger
        )
        {
        }

        public ObjectCache(
            Func<TKey, Task<TValue>> factory,
            Func<TValue, Task> suspend,
            Func<TValue, Task> resume,
            ILogger<ObjectCache<TKey, TValue>> logger
        )
        {
            _factory = factory;
            _suspend = suspend;
            _resume = resume;
            Logger = logger;
        }

        public ObjectCache(
            Func<TKey, Task<ICacheReference<TValue>>> externalFactory,
            ILogger<ObjectCache<TKey, TValue>> logger
        ) : this(
            externalFactory,
            _ => Task.CompletedTask,
            _ => Task.CompletedTask,
            logger
        )
        {
        }

        public ObjectCache(
            Func<TKey, Task<ICacheReference<TValue>>> externalFactory,
            Func<TValue, Task> suspend,
            Func<TValue, Task> resume,
            ILogger<ObjectCache<TKey, TValue>> logger
        )
        {
            _externalFactory = externalFactory;
            _suspend = suspend;
            _resume = resume;
            Logger = logger;
        }

        public async Task<ICacheReference<TValue>> GetAsync(TKey key)
        {
            // get or create CacheEntry
            CacheEntry entry;
            var isInitializing = false;
            lock (_entries)
            {
                if (_entries.TryGetValue(key, out entry!))
                    this.Trace($"Get by {key}: entry already exists");
                else
                {
                    this.Trace($"Get by {key}: entry missed, creating");
                    entry = _entries[key] = new CacheEntry();
                    isInitializing = true;
                }
            }

            // creator - immediately creates value, others - wait for access
            ICacheReference<TValue>? reference = null;
            if (isInitializing)
            {
                this.Trace($"Get by {key}: initialize entry");
                if (_factory != null)
                    entry.SetValue(await _factory(key));
                else if (_externalFactory != null)
                {
                    reference = await _externalFactory(key);
                    entry.SetValue(reference.Value);
                }
            }
            else
            {
                this.Trace($"Get by {key}: wait entry");
                entry.Wait();
            }

            // if not initializing and entry has no references - it is suspended, need to resume
            if (!isInitializing && !entry.HasReferences)
            {
                this.Trace($"Get by {key}: resume entry");
                await _resume(entry.Value);
            }

            // create reference, incrementing reference counter
            this.Trace($"Get by {key}: add entry reference");
            entry.AddReference();
            if (reference is null)
                reference = new CacheReference<TValue>(entry.Value, () => Release(key, entry));

            entry.Unlock();

            return reference;
        }

        private async Task Release(TKey key, CacheEntry entry)
        {
            this.Trace($"Release by {key}: wait entry");
            entry.Wait();

            this.Trace($"Release by {key}: remove reference");
            entry.RemoveReference();
            if (!entry.HasReferences)
            {
                this.Trace($"Release by {key}: suspend entry");
                await _suspend(entry.Value);
            }

            entry.Unlock();
        }

        public async ValueTask DisposeAsync()
        {
            KeyValuePair<TKey, CacheEntry>[] cacheEntries;
            lock (_entries)
            {
                cacheEntries = _entries.ToArray();
                _entries.Clear();
            }

            this.Trace($"Dispose cache: {cacheEntries.Length} entries");
            foreach (var (_, entry) in cacheEntries)
                await entry.DisposeAsync();
        }

        private class CacheEntry : IAsyncDisposable
        {
            public TValue Value { get; private set; } = default!;
            public bool HasReferences => _references != 0;

            private readonly AutoResetEvent _gate = new(initialState: false);
            private uint _references;

            public void Wait() => _gate.WaitOne();
            public void Unlock() => _gate.Set();

            public void SetValue(TValue value)
            {
                if (Value?.Equals(default) ?? true)
                    Value = value;
                else
                    throw new InvalidOperationException("Can't change CacheEntry Value");
            }

            public void AddReference() => ++_references;
            public void RemoveReference() => --_references;

            public async ValueTask DisposeAsync()
            {
                _gate.Reset();
                _gate.Dispose();

                switch (Value)
                {
                    case IAsyncDisposable asyncDisposable:
                        await asyncDisposable.DisposeAsync();
                        break;
                    case IDisposable disposable:
                        disposable.Dispose();
                        break;
                }
            }
        }
    }
}