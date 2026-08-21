using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Cache.Abstractions;

/// <summary>
/// Extension methods for cache operations
/// </summary>
public static class CacheExtensions
{
    /// <summary>
    /// Gets an existing item from cache or creates a new one using the provided factory
    /// </summary>
    /// <typeparam name="TKey">Type of the cache key</typeparam>
    /// <typeparam name="TValue">Type of the cached value</typeparam>
    /// <param name="cache">The cache instance</param>
    /// <param name="key">The cache key</param>
    /// <param name="factory">Factory function to create the value if not found in cache</param>
    /// <param name="options">Cache options including expiration settings</param>
    /// <param name="ct">Cancellation token for the awaiting caller</param>
    /// <returns>The cached or newly created value</returns>
    public static ValueTask<TValue> GetOrCreateAsync<TKey, TValue>(
        this ICache<TKey, TValue> cache,
        TKey key,
        Func<TKey, CancellationToken, ValueTask<TValue>> factory,
        CacheOptions options,
        CancellationToken ct = default
    )
        where TKey : IEquatable<TKey>
        where TValue : notnull
    {
        // The third parameter is supplied by the cache implementation when invoking the factory
        // (typically CancellationToken.None — factory work is shared across waiters and not bound to any single caller).
        static ValueTask<TValue> FactoryAsync(
            TKey key,
            Func<TKey, CancellationToken, ValueTask<TValue>> factory,
            CancellationToken factoryCt
        ) => factory(key, factoryCt);

        return cache.GetOrCreateAsync(key, FactoryAsync, factory, options, ct);
    }
}
