using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Cache.Abstractions;
using Annium.Logging;
using Annium.Redis;

namespace Annium.Cache.Redis.Internal;

/// <summary>
/// Redis-backed cache implementation of <see cref="ICache{TKey,TValue}"/>, built on the shared
/// <see cref="IRedisStorage"/> abstraction from <c>Annium.Redis</c>.
/// </summary>
/// <remarks>
/// The connection is owned by <see cref="IRedisStorage"/> (a DI-managed singleton), so this cache
/// holds no connection of its own. <see cref="GetOrCreateAsync{TContext}"/> — serialization, logical
/// expiry, and single-flight — is implemented in later tasks; this task wires storage access and the
/// key-namespacing/disposal lifecycle, and implements <see cref="RemoveAsync"/>.
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
    /// Disposal flag that ensures <see cref="DisposeAsync"/> is idempotent (0 = live, 1 = disposed).
    /// </summary>
    private int _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="Cache{TKey,TValue}"/> class.
    /// </summary>
    /// <param name="storage">The shared Redis storage.</param>
    /// <param name="options">The cache options.</param>
    /// <param name="logger">The logger.</param>
    public Cache(IRedisStorage storage, RedisCacheOptions options, ILogger logger)
    {
        _storage = storage;
        _options = options;
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
    public ValueTask<TValue> GetOrCreateAsync<TContext>(
        TKey key,
        Func<TKey, TContext, CancellationToken, ValueTask<TValue>> factory,
        TContext context,
        CacheOptions options,
        CancellationToken ct = default
    )
        where TContext : notnull
    {
        throw new NotImplementedException();
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

        await _storage.DeleteAsync(Key(key), ct);
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
    /// Builds the namespaced Redis key for a cache key by prepending the configured prefix.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <returns>The prefixed Redis key string.</returns>
    private string Key(TKey key) => _options.KeyPrefix + key;
}
