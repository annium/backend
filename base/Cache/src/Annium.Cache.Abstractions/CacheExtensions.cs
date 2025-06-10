using System;
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
    /// <param name="cache">The cache instance</param>
    /// <param name="key">The cache key</param>
    /// <param name="factory">Factory function to create the value if not found in cache</param>
    /// <param name="options">Cache options including expiration settings</param>
    /// <returns>The cached or newly created value</returns>
    public static ValueTask<TValue> GetOrCreateAsync<TKey, TValue>(
        this ICache<TKey, TValue> cache,
        TKey key,
        Func<TKey, ValueTask<TValue>> factory,
        CacheOptions options
    )
        where TKey : IEquatable<TKey>
        where TValue : notnull
    {
        static ValueTask<TValue> FactoryAsync(TKey key, Func<TKey, ValueTask<TValue>> factory) => factory(key);

        return cache.GetOrCreateAsync(key, FactoryAsync, factory, options);
    }
}
