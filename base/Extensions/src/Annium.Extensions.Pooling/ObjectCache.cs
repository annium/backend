using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Annium.Extensions.Pooling
{
    public class ObjectCache<TKey, TValue> : IObjectCache<TKey, TValue>
        where TKey : notnull
        where TValue : notnull
    {
        private readonly IDictionary<TKey, CacheEntry> entries = new Dictionary<TKey, CacheEntry>();
        private readonly Func<TKey, Task<TValue>>? factory;
        private readonly Func<TKey, Task<ICacheReference<TValue>>>? externalFactory;
        private readonly Func<TValue, Task> suspend;
        private readonly Func<TValue, Task> resume;
        private readonly ILogger<ObjectCache<TKey, TValue>> logger;

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
            this.factory = factory;
            this.suspend = suspend;
            this.resume = resume;
            this.logger = logger;
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
            this.externalFactory = externalFactory;
            this.suspend = suspend;
            this.resume = resume;
            this.logger = logger;
        }

        public async Task<ICacheReference<TValue>> GetAsync(TKey key)
        {
            // get or create CacheEntry
            CacheEntry entry;
            var isInitializing = false;
            lock (entries)
            {
                if (entries.TryGetValue(key, out entry!))
                    Trace($"Get by {key}: entry already exists");
                else
                {
                    Trace($"Get by {key}: entry missed, creating");
                    entry = entries[key] = new CacheEntry();
                    isInitializing = true;
                }
            }

            // creator - immediately creates value, others - wait for access
            ICacheReference<TValue>? reference = null;
            if (isInitializing)
            {
                Trace($"Get by {key}: initialize entry");
                if (factory != null)
                    entry.SetValue(await factory(key));
                else if (externalFactory != null)
                {
                    reference = await externalFactory(key);
                    entry.SetValue(reference.Value);
                }
            }
            else
            {
                Trace($"Get by {key}: wait entry");
                entry.Wait();
            }

            // if not initializing and entry has no references - it is suspended, need to resume
            if (!isInitializing && !entry.HasReferences)
            {
                Trace($"Get by {key}: resume entry");
                await resume(entry.Value);
            }

            // create reference, incrementing reference counter
            Trace($"Get by {key}: add entry reference");
            entry.AddReference();
            if (reference is null)
                reference = new CacheReference<TValue>(entry.Value, () => Release(key, entry));

            entry.Unlock();

            return reference;
        }

        private async Task Release(TKey key, CacheEntry entry)
        {
            Trace($"Release by {key}: wait entry");
            entry.Wait();

            Trace($"Release by {key}: remove reference");
            entry.RemoveReference();
            if (!entry.HasReferences)
            {
                Trace($"Release by {key}: suspend entry");
                await suspend(entry.Value);
            }

            entry.Unlock();
        }

        private void Trace(string message)
        {
            logger.Trace($"[{Thread.CurrentThread.ManagedThreadId,3:D}] {message}");
        }

        public async ValueTask DisposeAsync()
        {
            IList<KeyValuePair<TKey, CacheEntry>> cacheEntries;
            lock (entries)
            {
                cacheEntries = entries.ToList();
                entries.Clear();
            }

            Trace($"Dispose cache: {cacheEntries.Count} entries");
            foreach (var (_, entry) in cacheEntries)
                await entry.DisposeAsync();
            cacheEntries.Clear();
        }

        private class CacheEntry : IAsyncDisposable
        {
            public TValue Value { get; private set; } = default!;
            public bool HasReferences => references != 0;

            private readonly AutoResetEvent gate = new AutoResetEvent(initialState: false);
            private uint references = 0;

            public void Wait() => gate.WaitOne();
            public void Unlock() => gate.Set();

            public void SetValue(TValue value)
            {
                if (Value?.Equals(default) ?? true)
                    Value = value;
                else
                    throw new InvalidOperationException($"Can't change CacheEntry Value");
            }

            public void AddReference() => ++references;
            public void RemoveReference() => --references;

            public async ValueTask DisposeAsync()
            {
                gate.Reset();
                gate.Dispose();

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