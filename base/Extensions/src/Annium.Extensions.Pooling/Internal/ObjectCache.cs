using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Annium.Extensions.Pooling.Internal;

internal sealed class ObjectCache<TKey, TValue> : IObjectCache<TKey, TValue>, ILogSubject<ObjectCache<TKey, TValue>>
    where TKey : notnull
    where TValue : class
{
    private readonly ObjectCacheProvider<TKey, TValue> _provider;
    public ILogger<ObjectCache<TKey, TValue>> Logger { get; }
    private readonly IDictionary<TKey, CacheEntry> _entries = new Dictionary<TKey, CacheEntry>();

    public ObjectCache(
        ObjectCacheProvider<TKey, TValue> provider,
        ILogger<ObjectCache<TKey, TValue>> logger
    )
    {
        _provider = provider;
        Logger = logger;
    }

    public async Task<ICacheReference<TValue>> GetAsync(TKey key, CancellationToken ct = default)
    {
        // get or create CacheEntry
        CacheEntry entry;
        var isInitializing = false;
        lock (_entries)
        {
            if (_entries.TryGetValue(key, out entry!))
                this.Log().Trace($"Get by {key}: entry already exists");
            else
            {
                this.Log().Trace($"Get by {key}: entry missed, creating");
                entry = _entries[key] = new CacheEntry();
                isInitializing = true;
            }
        }

        // creator - immediately creates value, others - wait for access
        ICacheReference<TValue>? reference = null;
        if (isInitializing)
        {
            this.Log().Trace($"Get by {key}: initialize entry");
            if (_provider.HasCreate)
                entry.SetValue(await _provider.CreateAsync(key, ct));
            else if (_provider.HasExternalCreate)
            {
                reference = await _provider.ExternalCreateAsync(key, ct);
                entry.SetValue(reference.Value);
            }
            else
                throw new NotImplementedException("Neither base not external factory is implemented");

            this.Log().Trace($"Get by {key}: entry ready");
        }
        else
        {
            this.Log().Trace($"Get by {key}: wait entry");
            await entry.WaitAsync();
        }

        // if not initializing and entry has no references - it is suspended, need to resume
        if (!isInitializing && !entry.HasReferences)
        {
            this.Log().Trace($"Get by {key}: resume entry");
            await _provider.ResumeAsync(entry.Value);
        }

        // create reference, incrementing reference counter
        this.Log().Trace($"Get by {key}: add entry reference");
        entry.AddReference();
        reference ??= new CacheReference<TValue>(entry.Value, () => Release(key, entry));

        entry.Release();

        return reference;
    }

    private async Task Release(TKey key, CacheEntry entry)
    {
        this.Log().Trace($"Release by {key}: wait entry");
        await entry.WaitAsync();

        this.Log().Trace($"Release by {key}: remove reference");
        entry.RemoveReference();
        if (!entry.HasReferences)
        {
            this.Log().Trace($"Release by {key}: suspend entry");
            await _provider.SuspendAsync(entry.Value);
        }

        entry.Release();
    }

    public async ValueTask DisposeAsync()
    {
        this.Log().Trace("start");

        KeyValuePair<TKey, CacheEntry>[] cacheEntries;
        lock (_entries)
        {
            cacheEntries = _entries.ToArray();
            _entries.Clear();
        }

        this.Log().Trace("dispose {count} entries", cacheEntries.Length);

        foreach (var (_, entry) in cacheEntries)
        {
            this.Log().Trace("dispose {entry} entries", entry);
            await entry.DisposeAsync();
        }

        this.Log().Trace("done");
    }

    private sealed record CacheEntry : IAsyncDisposable
    {
        public TValue Value => _value ?? throw new InvalidOperationException("Value is not set");
        public bool HasReferences => _references != 0;

        private readonly AutoResetEvent _gate = new(initialState: false);
        private TValue? _value;
        private uint _references;

        public Task WaitAsync() => Task.Run(() => _gate.WaitOne());

        public void Release() => _gate.Set();

        public void SetValue(TValue value)
        {
            if (_value is null)
                _value = value;
            else
                throw new InvalidOperationException("Can't change CacheEntry Value");
        }

        public void AddReference() => ++_references;
        public void RemoveReference() => --_references;

        public override string ToString() => $"{Value} [{_references}]";

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