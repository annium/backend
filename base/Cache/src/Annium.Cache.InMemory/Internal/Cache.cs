using System;
using System.Collections.Generic;
using System.Threading;
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
internal class Cache<TKey, TValue> : ICache<TKey, TValue>, ILogSubject
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

    /// <summary>
    /// Disposal flag set via Interlocked.Exchange to make DisposeAsync idempotent under races
    /// </summary>
    private int _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="Cache{TKey, TValue}"/> class.
    /// </summary>
    /// <param name="timeProvider">Time provider used to evaluate entry expiration.</param>
    /// <param name="logger">Logger used for tracing.</param>
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
    /// <typeparam name="TContext">Type of the state the factory receives, so creating a value needs no closure</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="factory">Factory function to create the value if not found in cache</param>
    /// <param name="context">Context object passed to the factory function</param>
    /// <param name="options">Cache options including expiration settings</param>
    /// <param name="ct">Cancellation token for the awaiting caller</param>
    /// <returns>The cached or newly created value</returns>
    public async ValueTask<TValue> GetOrCreateAsync<TContext>(
        TKey key,
        Func<TKey, TContext, CancellationToken, ValueTask<TValue>> factory,
        TContext context,
        CacheOptions options,
        CancellationToken ct = default
    )
        where TContext : notnull
    {
        EnsureUsable(ct);

        var entry = GetOrCreateEntry(key, factory, context, options);
        // VSTHRD003: Tcs.Task is the cache's own shared per-key work, not a foreign task; awaiting it
        // via WaitAsync(ct) is how each caller observes its own cancellation without faulting the shared TCS.
#pragma warning disable VSTHRD003
        return await entry.Tcs.Task.WaitAsync(ct);
#pragma warning restore VSTHRD003
    }

    /// <summary>
    /// Removes an item from the cache
    /// </summary>
    /// <param name="key">The cache key to remove</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A value task that represents the asynchronous remove operation</returns>
    public ValueTask RemoveAsync(TKey key, CancellationToken ct = default)
    {
        EnsureUsable(ct);

        lock (_data)
            _data.Remove(key);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Throws if the cache is disposed or the caller's token is already cancelled.
    /// </summary>
    /// <param name="ct">The caller's cancellation token.</param>
    private void EnsureUsable(CancellationToken ct)
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);
        ct.ThrowIfCancellationRequested();
    }

    /// <summary>
    /// Disposes the cache and its background executor (idempotent)
    /// </summary>
    /// <returns>A task representing the disposal operation</returns>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        await _executor.DisposeAsync();
    }

    /// <summary>
    /// Gets or creates a cache entry with expiration handling. The factory runs as shared work
    /// in the background executor and is not bound to any single caller's CancellationToken;
    /// per-caller cancellation is enforced by Task.WaitAsync(ct) in GetOrCreateAsync.
    /// </summary>
    /// <typeparam name="TContext">Type of the state the factory receives, so creating a value needs no closure</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="factory">Factory function to create the value</param>
    /// <param name="context">Context object passed to the factory function</param>
    /// <param name="options">Cache options including expiration settings</param>
    /// <returns>The cache entry</returns>
    private Entry GetOrCreateEntry<TContext>(
        TKey key,
        Func<TKey, TContext, CancellationToken, ValueTask<TValue>> factory,
        TContext context,
        CacheOptions options
    )
    {
        lock (_data)
        {
            var now = _timeProvider.Now;
            // First-writer-wins: extend an existing entry using the OPTIONS it was created with
            // (entry.Options), not the current caller's, so a later caller cannot override the
            // expiration strategy established for this key.
            if (_data.TryGetValue(key, out var entry) && entry.ExpiresAt > now)
                return entry.WithExpiresAt(entry.Options.GetExpiresAt(now));

            this.Trace("Create item for {key}", key);

            // RunContinuationsAsynchronously: TrySet* below runs inside the executor lambda, so without this
            // every awaiting caller's continuation would resume inline on the executor thread, serializing
            // resumes and blocking the lambda (and thus the executor drain / DisposeAsync) until they finish.
            var tcs = new TaskCompletionSource<TValue>(TaskCreationOptions.RunContinuationsAsynchronously);
            var created = _data[key] = new Entry(tcs, options, options.GetExpiresAt(now));

            var scheduled = _executor.Schedule(async () =>
            {
                this.Trace("Get {key} value", key);
                try
                {
                    var value = await factory(key, context, CancellationToken.None);

                    // Re-check expiry against the entry's LIVE ExpiresAt (read under the lock) so a
                    // concurrent sliding-expiration hit that prolonged the window is honored; the
                    // creation-time window would wrongly discard a still-valid, just-extended entry.
                    // Evict only when the slot still holds OUR entry, so a concurrently-created
                    // replacement for the same key is not dropped (both done under one lock).
                    bool live;
                    lock (_data)
                    {
                        live = created.ExpiresAt > _timeProvider.Now;
                        if (!live && ReferenceEquals(_data.GetValueOrDefault(key), created))
                            _data.Remove(key);
                    }

                    if (live)
                        tcs.TrySetResult(value);
                    else
                        tcs.TrySetCanceled();
                }
                catch (Exception ex)
                {
                    this.Trace("Factory failed for {key}", key);
                    this.Error(ex);
                    lock (_data)
                        if (ReferenceEquals(_data.GetValueOrDefault(key), created))
                            _data.Remove(key);
                    tcs.TrySetException(ex);
                }
            });

            if (!scheduled)
            {
                // The executor was disposed concurrently (TOCTOU with DisposeAsync, between the disposed
                // guard and here): the scheduled factory will never run. Roll back the just-inserted entry
                // and surface disposal so the caller fails fast instead of awaiting a TCS that never resolves.
                _data.Remove(key);
                throw new ObjectDisposedException(nameof(Cache<,>));
            }

            return created;
        }
    }

    /// <summary>
    /// Cache entry holding the task completion source, the options it was created with, and its expiration time.
    /// A plain class (not a record): identity is what matters (eviction is ReferenceEquals-guarded) and
    /// <see cref="ExpiresAt"/> is mutated in-place under the cache lock, so value-equality semantics would mislead.
    /// </summary>
    /// <param name="tcs">Task completion source for the cached value</param>
    /// <param name="options">The options this entry was created with; used to extend the window on each hit</param>
    /// <param name="expiresAt">Initial expiration time</param>
    private sealed class Entry(TaskCompletionSource<TValue> tcs, CacheOptions options, Instant expiresAt)
    {
        /// <summary>
        /// Task completion source for the cached value
        /// </summary>
        public TaskCompletionSource<TValue> Tcs { get; } = tcs;

        /// <summary>
        /// The options this entry was created with, used to compute the extended expiry on each hit
        /// </summary>
        public CacheOptions Options { get; } = options;

        /// <summary>
        /// The expiration time for this cache entry
        /// </summary>
        public Instant ExpiresAt { get; private set; } = expiresAt;

        /// <summary>
        /// Updates the expiration time for this entry
        /// </summary>
        /// <param name="value">New expiration time</param>
        /// <returns>This entry instance for chaining</returns>
        public Entry WithExpiresAt(Instant value)
        {
            ExpiresAt = value;

            return this;
        }
    }
}
