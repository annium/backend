using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Annium.Cache.Abstractions;
using Annium.Core.Runtime.Time;
using Annium.Logging;
using Annium.Redis;
using Annium.Serialization.Abstractions;
using NodaTime;

namespace Annium.Cache.Redis.Internal;

/// <summary>
/// Redis-backed cache implementation of <see cref="ICache{TKey,TValue}"/>, built on the shared
/// <see cref="IRedisStorage"/> abstraction from <c>Annium.Redis</c>.
/// </summary>
/// <remarks>
/// The connection is owned by <see cref="IRedisStorage"/> (a DI-managed singleton), so this cache holds
/// no connection of its own. Expiry is enforced <em>logically</em> via <see cref="ITimeProvider"/> (the
/// stored envelope carries an absolute deadline) so that managed-time tests and the InMemory contract
/// stay aligned; Redis' physical TTL is a secondary leak-guard. Concurrent callers for the same missing
/// key are de-duplicated in-process (single-flight), so the factory runs once per cache instance.
/// Sliding refresh and the finer in-flight cancel/drain semantics are added in later tasks.
/// </remarks>
/// <typeparam name="TKey">The type of cache keys.</typeparam>
/// <typeparam name="TValue">The type of cached values.</typeparam>
internal class Cache<TKey, TValue> : ICache<TKey, TValue>, ILogSubject
    where TKey : IEquatable<TKey>
    where TValue : notnull
{
    /// <summary>
    /// Gets the logger for this cache.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The shared Redis storage backing this cache.
    /// </summary>
    private readonly IRedisStorage _storage;

    /// <summary>
    /// Cache-level options (notably the key prefix).
    /// </summary>
    private readonly RedisCacheOptions _options;

    /// <summary>
    /// Serializer used to encode/decode the stored <see cref="CacheEnvelope{TValue}"/>.
    /// </summary>
    private readonly ISerializer<string> _serializer;

    /// <summary>
    /// Time provider driving logical expiry.
    /// </summary>
    private readonly ITimeProvider _timeProvider;

    /// <summary>
    /// In-process single-flight map: concurrent callers for the same key share one factory run.
    /// </summary>
    private readonly ConcurrentDictionary<string, Flight> _inflight = new();

    /// <summary>
    /// Disposal flag that ensures <see cref="DisposeAsync"/> is idempotent (0 = live, 1 = disposed).
    /// </summary>
    private int _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="Cache{TKey,TValue}"/> class.
    /// </summary>
    /// <param name="storage">The shared Redis storage.</param>
    /// <param name="options">The cache options.</param>
    /// <param name="serializer">The serializer for cache envelopes.</param>
    /// <param name="timeProvider">The time provider driving logical expiry.</param>
    /// <param name="logger">The logger.</param>
    public Cache(
        IRedisStorage storage,
        RedisCacheOptions options,
        ISerializer<string> serializer,
        ITimeProvider timeProvider,
        ILogger logger
    )
    {
        _storage = storage;
        _options = options;
        _serializer = serializer;
        _timeProvider = timeProvider;
        Logger = logger;
    }

    /// <summary>
    /// Gets an existing item from the cache or creates a new one using the provided factory.
    /// </summary>
    /// <typeparam name="TContext">The type of the context object passed to the factory.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="factory">Factory function to create the value if not found in cache.</param>
    /// <param name="context">Context object passed to the factory function.</param>
    /// <param name="options">Cache options including expiration settings.</param>
    /// <param name="ct">Cancellation token for the awaiting caller.</param>
    /// <returns>The cached or newly created value.</returns>
    public async ValueTask<TValue> GetOrCreateAsync<TContext>(
        TKey key,
        Func<TKey, TContext, CancellationToken, ValueTask<TValue>> factory,
        TContext context,
        CacheOptions options,
        CancellationToken ct = default
    )
        where TContext : notnull
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);
        ct.ThrowIfCancellationRequested();

        var k = Key(key);

        // in-process single-flight: install the shared Flight SYNCHRONOUSLY (before any await) so a
        // concurrent RemoveAsync deterministically observes an in-flight creation and can invalidate it.
        // The caller that installed it runs the read+factory DETACHED (so its own cancellation doesn't
        // block the shared work); every caller — winner and losers alike — awaits the shared task via
        // WaitAsync, observing per-caller cancellation. The read (hit / cross-process / sliding refresh)
        // happens inside the winner's CreateAsync, so a hit still avoids invoking the factory.
        var tcs = new TaskCompletionSource<TValue>(TaskCreationOptions.RunContinuationsAsynchronously);
        var flight = new Flight { Task = tcs.Task };
        var existing = _inflight.GetOrAdd(k, flight);
        if (existing == flight)
        {
            // fire-and-forget: RunFactoryAsync always settles the tcs, so no exception is unobserved.
            _ = RunFactoryAsync(k, key, factory, context, options, flight, tcs);
        }

        // VSTHRD003: the shared task is this cache's own single-flight task, not a foreign one.
#pragma warning disable VSTHRD003
        return await existing.Task.WaitAsync(ct);
#pragma warning restore VSTHRD003
    }

    /// <summary>
    /// Runs the factory for the single-flight winner detached from any caller, always settling the shared
    /// task and unpoisoning the in-flight slot so a later call re-reads / re-creates.
    /// </summary>
    /// <typeparam name="TContext">The type of the context object passed to the factory.</typeparam>
    /// <param name="k">The prefixed Redis key.</param>
    /// <param name="key">The cache key.</param>
    /// <param name="factory">Factory function to create the value.</param>
    /// <param name="context">Context object passed to the factory function.</param>
    /// <param name="options">Cache options including expiration settings.</param>
    /// <param name="flight">The in-flight holder for this key (carries the shared task and the invalidation flag).</param>
    /// <param name="tcs">The task completion source that all callers await.</param>
    /// <returns>A task that completes when the factory run has settled the shared task.</returns>
    private async Task RunFactoryAsync<TContext>(
        string k,
        TKey key,
        Func<TKey, TContext, CancellationToken, ValueTask<TValue>> factory,
        TContext context,
        CacheOptions options,
        Flight flight,
        TaskCompletionSource<TValue> tcs
    )
        where TContext : notnull
    {
        try
        {
            tcs.TrySetResult(await CreateAsync(k, key, factory, context, options, flight));
        }
        catch (Exception ex)
        {
            this.Trace("Factory failed for {key}", key);
            this.Error(ex);
            tcs.TrySetException(ex);
        }
        finally
        {
            _inflight.TryRemove(new KeyValuePair<string, Flight>(k, flight));
        }
    }

    /// <summary>
    /// Removes an item from the cache.
    /// </summary>
    /// <param name="key">The cache key to remove.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A value task that represents the asynchronous remove operation.</returns>
    public async ValueTask RemoveAsync(TKey key, CancellationToken ct = default)
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);
        ct.ThrowIfCancellationRequested();

        var k = Key(key);

        // invalidate any in-flight creation for this key so its (post-factory) write is suppressed —
        // a remove during creation must purge the entry, not race a stale write back in.
        if (_inflight.TryGetValue(k, out var flight))
            flight.Invalidated = true;

        await _storage.DeleteAsync(k, ct);
    }

    /// <summary>
    /// Disposes the cache. Idempotent. The underlying <see cref="IRedisStorage"/> connection is owned
    /// by the DI container, so there is nothing to release here beyond flipping the disposed flag.
    /// </summary>
    /// <returns>A completed <see cref="ValueTask"/>.</returns>
    public ValueTask DisposeAsync()
    {
        Interlocked.Exchange(ref _disposed, 1);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Reads the stored entry for a key and returns it only if it is logically live (not past its deadline).
    /// </summary>
    /// <param name="k">The prefixed Redis key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A tuple of (hit, value); <c>hit</c> is false on a missing or logically-expired entry.</returns>
    private async Task<(bool Hit, TValue Value)> ReadLiveAsync(string k, CancellationToken ct)
    {
        var raw = await _storage.GetAsync(k, ct);
        if (raw is null)
            return (false, default!);

        var env = _serializer.Deserialize<CacheEnvelope<TValue>>(raw);
        var now = _timeProvider.Now;
        if (now.ToUnixTimeMilliseconds() >= env.ExpiresAtMs)
            return (false, default!);

        // sliding refresh: prolong the window on access using the STORED lifetime (first-writer-wins —
        // a later caller's options must not change the entry's strategy). No atomic GETEX on IRedisStorage,
        // so this is a GET (above) + SET (2 RTT); a benign race between concurrent readers prolongs alike.
        if (env.Mode == CacheExpirationMode.Sliding && env.LifetimeMs is { } lifetimeMs)
        {
            var lifetime = Duration.FromMilliseconds(lifetimeMs);
            var refreshed = env with { ExpiresAtMs = (now + lifetime).ToUnixTimeMilliseconds() };
            await _storage.SetAsync(k, _serializer.Serialize(refreshed), lifetime, ct);
        }

        return (true, env.Value);
    }

    /// <summary>
    /// The single-flight winner's work: re-check the store, invoke the factory, and write the value with
    /// its expiry envelope. The factory runs detached from any single caller's cancellation.
    /// </summary>
    /// <typeparam name="TContext">The type of the context object passed to the factory.</typeparam>
    /// <param name="k">The prefixed Redis key.</param>
    /// <param name="key">The cache key.</param>
    /// <param name="factory">Factory function to create the value.</param>
    /// <param name="context">Context object passed to the factory function.</param>
    /// <param name="options">Cache options including expiration settings.</param>
    /// <param name="flight">The in-flight holder; its invalidation flag suppresses the write-back after a concurrent remove.</param>
    /// <returns>A task that resolves to the cached or newly created value.</returns>
    private async Task<TValue> CreateAsync<TContext>(
        string k,
        TKey key,
        Func<TKey, TContext, CancellationToken, ValueTask<TValue>> factory,
        TContext context,
        CacheOptions options,
        Flight flight
    )
        where TContext : notnull
    {
        // read: return a live stored value (own earlier write, a cross-process write, or a sliding hit
        // which ReadLiveAsync also refreshes) without invoking the factory.
        var (hit, value) = await ReadLiveAsync(k, CancellationToken.None);
        if (hit)
            return value;

        this.Trace("Create item for {key}", key);
        value = await factory(key, context, CancellationToken.None);

        // A RemoveAsync that ran while the factory was in flight invalidated this creation — return the
        // produced value to the awaiting callers but do NOT write it back, so the key stays purged.
        if (flight.Invalidated)
            return value;

        var now = _timeProvider.Now;
        var expiresAt = options.GetExpiresAt(now);
        var envelope = new CacheEnvelope<TValue>
        {
            Value = value,
            Mode = options.Mode,
            ExpiresAtMs = expiresAt.ToUnixTimeMilliseconds(),
            LifetimeMs = options.Mode == CacheExpirationMode.Sliding ? (long)options.Lifetime.TotalMilliseconds : null,
        };

        // physical TTL as a leak-guard; logical expiry (ExpiresAtMs vs ITimeProvider.Now) is authoritative.
        var ttl = expiresAt - now;
        if (ttl <= Duration.Zero)
            ttl = Duration.FromMilliseconds(1);

        await _storage.SetAsync(k, _serializer.Serialize(envelope), ttl, CancellationToken.None);

        // a RemoveAsync that landed during the write (after the pre-write check above, before the SET
        // completed) must still win — delete the just-written entry so a concurrent remove is not lost.
        if (flight.Invalidated)
            await _storage.DeleteAsync(k, CancellationToken.None);

        return value;
    }

    /// <summary>
    /// A single in-flight factory run for a key. Carries the shared task all callers await, plus an
    /// invalidation flag set by <see cref="RemoveAsync"/> to suppress a post-factory write-back.
    /// </summary>
    private sealed class Flight
    {
        /// <summary>
        /// Gets the shared task that resolves to the created value (or faults with the factory's exception).
        /// </summary>
        public required Task<TValue> Task { get; init; }

        /// <summary>
        /// Whether a concurrent remove invalidated this creation; when set, the winner skips the write-back.
        /// </summary>
        public volatile bool Invalidated;
    }

    /// <summary>
    /// Builds the namespaced Redis key for a cache key by prepending the configured prefix.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <returns>The prefixed Redis key string.</returns>
    private string Key(TKey key) => _options.KeyPrefix + key;
}
