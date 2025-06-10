using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Cache.Abstractions;
using Annium.Execution.Background;
using Annium.Logging;
using NodaTime;

namespace Annium.Cache.InMemory.Internal;

/// <summary>
/// In-memory cache implementation that stores values with expiration support
/// </summary>
/// <typeparam name="TKey">The type of cache keys</typeparam>
/// <typeparam name="TValue">The type of cached values</typeparam>
internal class Cache<TKey, TValue> : ICache<TKey, TValue>, IAsyncDisposable, ILogSubject
    where TKey : IEquatable<TKey>
    where TValue : notnull
{
    /// <summary>
    /// Logger instance for this cache
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Time provider for expiration calculations
    /// </summary>
    private readonly ITimeProvider _timeProvider;

    /// <summary>
    /// Dictionary storing cached entries
    /// </summary>
    private readonly Dictionary<TKey, Entry> _data = new();

    /// <summary>
    /// Background executor for async operations
    /// </summary>
    private readonly IExecutor _executor;

    public Cache(ITimeProvider timeProvider, ILogger logger)
    {
        _timeProvider = timeProvider;
        _executor = Executor.Concurrent<Cache<TKey, TValue>>(logger);
        _executor.Start();
        Logger = logger;
    }

    /// <summary>
    /// Gets an existing item from cache or creates a new one using the provided factory
    /// </summary>
    /// <param name="key">The cache key</param>
    /// <param name="factory">Factory function to create the value if not found in cache</param>
    /// <param name="context">Context object passed to the factory function</param>
    /// <param name="options">Cache options including expiration settings</param>
    /// <returns>The cached or newly created value</returns>
    public async ValueTask<TValue> GetOrCreateAsync<TContext>(
        TKey key,
        Func<TKey, TContext, ValueTask<TValue>> factory,
        TContext context,
        CacheOptions options
    )
        where TContext : notnull
    {
#pragma warning disable VSTHRD003
        return await GetOrCreateEntry(key, factory, context, options).Tcs.Task;
#pragma warning restore VSTHRD003
    }

    /// <summary>
    /// Removes an item from the cache
    /// </summary>
    /// <param name="key">The cache key to remove</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public ValueTask RemoveAsync(TKey key)
    {
        lock (_data)
            _data.Remove(key);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Disposes the cache and its background executor
    /// </summary>
    /// <returns>A task representing the disposal operation</returns>
    public async ValueTask DisposeAsync()
    {
        await _executor.DisposeAsync();
    }

    /// <summary>
    /// Gets or creates a cache entry with expiration handling
    /// </summary>
    /// <param name="key">The cache key</param>
    /// <param name="factory">Factory function to create the value</param>
    /// <param name="context">Context object passed to the factory function</param>
    /// <param name="options">Cache options including expiration settings</param>
    /// <returns>The cache entry</returns>
    private Entry GetOrCreateEntry<TContext>(
        TKey key,
        Func<TKey, TContext, ValueTask<TValue>> factory,
        TContext context,
        CacheOptions options
    )
    {
        lock (_data)
        {
            var now = _timeProvider.Now;
            if (_data.TryGetValue(key, out var entry) && entry.ExpiresAt > now)
                return entry.WithExpiresAt(options.GetExpiresAt(now));

            this.Trace("Create item for {key}", key);

            var tcs = new TaskCompletionSource<TValue>();
            var expiresAt = options.GetExpiresAt(now);

            _executor.Schedule(async () =>
            {
                this.Trace("Get {key} value", key);
                var value = await factory(key, context);

                if (expiresAt > _timeProvider.Now)
                    tcs.SetResult(value);
                else
                    lock (_data)
                        _data.Remove(key);
            });

            return _data[key] = new(tcs, expiresAt);
        }
    }

    /// <summary>
    /// Cache entry containing the task completion source and expiration time
    /// </summary>
    /// <param name="Tcs">Task completion source for the cached value</param>
    /// <param name="ExpiresAt">Initial expiration time</param>
    private sealed record Entry(TaskCompletionSource<TValue> Tcs, Instant ExpiresAt)
    {
        /// <summary>
        /// The expiration time for this cache entry
        /// </summary>
        public Instant ExpiresAt { get; private set; } = ExpiresAt;

        /// <summary>
        /// Updates the expiration time for this entry
        /// </summary>
        /// <param name="expiresAt">New expiration time</param>
        /// <returns>This entry instance for chaining</returns>
        public Entry WithExpiresAt(Instant expiresAt)
        {
            ExpiresAt = expiresAt;

            return this;
        }
    }
}
