using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Extensions.Pooling
{
    public class ObjectCache<TKey, TValue> : IObjectCache<TKey, TValue>
    where TKey : notnull
    where TValue : notnull
    {
        private readonly Func<TKey, Task<TValue>> factory;
        private readonly Func<TValue, Task> suspend;
        private readonly Func<TValue, Task> resume;
        private readonly IDictionary<TKey, CacheEntry> entries = new Dictionary<TKey, CacheEntry>();

        public ObjectCache(
            Func<TKey, Task<TValue>> factory
        ) : this(
            factory,
            _ => Task.CompletedTask,
            _ => Task.CompletedTask
        )
        { }

        public ObjectCache(
            Func<TKey, Task<TValue>> factory,
            Func<TValue, Task> suspend,
            Func<TValue, Task> resume
        )
        {
            this.factory = factory;
            this.suspend = suspend;
            this.resume = resume;
        }

        public async Task<CacheReference<TValue>> GetAsync(TKey key)
        {
            // get or create CacheEntry
            CacheEntry entry;
            var isCreated = false;
            lock (entries)
                if (!entries.TryGetValue(key, out entry!))
                {
                    entry = entries[key] = new CacheEntry();
                    isCreated = true;
                }

            // create, if needed
            if (isCreated)
                await RunSafe(key, entry, CreateAsync);

            switch (entry.Status)
            {
                case CacheStatus.Creating:
                case CacheStatus.Resuming:
                    entry.Gate.Wait();
                    break;
                case CacheStatus.Active:
                    break;
                case CacheStatus.Suspending:
                    entry.Gate.Wait();
                    await RunSafe(key, entry, ResumeAsync);
                    break;
                case CacheStatus.Suspended:
                    await RunSafe(key, entry, ResumeAsync);
                    break;
            }

            return Reference(key, entry);
        }

        private async Task CreateAsync(TKey key, CacheEntry entry)
        {
            entry.Status = CacheStatus.Creating;
            var value = await factory(key);
            entry.SetValue(value);
            entry.Status = CacheStatus.Active;
        }

        private async Task ResumeAsync(TKey key, CacheEntry entry)
        {
            entry.Status = CacheStatus.Resuming;
            await resume(entry.Value);
            entry.Status = CacheStatus.Active;
        }

        private async Task SuspendAsync(TKey key, CacheEntry entry)
        {
            entry.Status = CacheStatus.Suspending;
            await suspend(entry.Value);
            entry.Status = CacheStatus.Suspended;
        }

        private void Release(TKey key, CacheEntry entry)
        {
            entry.RemoveReference();
            if (!entry.HasReferences)
                //TODO: doesn't look cool, but have no other idea of async Suspend from sync Dispose
                RunSafe(key, entry, SuspendAsync).GetAwaiter().GetResult();
        }

        private async Task RunSafe(
            TKey key,
            CacheEntry entry,
            Func<TKey, CacheEntry, Task> execute
        )
        {
            // set isExecuting flag to distinguish executor and awaiter threads
            var isExecuting = false;
            lock (entry)
                if (entry.Gate.IsSet)
                {
                    entry.Gate.Reset();
                    isExecuting = true;
                }

            // if not executing - wait and return, else - run
            if (!isExecuting)
            {
                entry.Gate.Wait();
                return;
            }

            // execute operation
            await execute(key, entry);

            entry.Gate.Set();
        }

        private CacheReference<TValue> Reference(TKey key, CacheEntry entry)
        {
            entry.AddReference();

            return new CacheReference<TValue>(entry.Value, () => Release(key, entry));
        }

        #region IDisposable support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (disposedValue)
                return;

            if (disposing)
            {
                foreach (var entry in entries.Values)
                    entry.Dispose();

                entries.Clear();
            }

            disposedValue = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        private class CacheEntry : IDisposable
        {
            public TValue Value { get; private set; } = default!;
            public ManualResetEventSlim Gate { get; } = new ManualResetEventSlim(initialState: true);
            public CacheStatus Status { get; set; }
            public bool HasReferences => references != 0;
            private uint references = 0;

            public void SetValue(TValue value)
            {
                if (Value?.Equals(default) ?? true)
                    Value = value;
                else
                    throw new InvalidOperationException($"Can't change CacheEntry Value");
            }
            public void AddReference() => ++references;
            public void RemoveReference() => --references;

            public void Dispose()
            {
                Gate.Reset();
                Gate.Dispose();
                (Value as IDisposable)?.Dispose();
            }
        }
    }
}