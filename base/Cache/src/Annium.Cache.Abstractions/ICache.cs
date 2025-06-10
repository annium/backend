using System;
using System.Threading.Tasks;

namespace Annium.Cache.Abstractions;

/// <summary>
/// Generic cache interface for storing and retrieving values by key
/// </summary>
/// <typeparam name="TKey">The type of cache keys</typeparam>
/// <typeparam name="TValue">The type of cached values</typeparam>
public interface ICache<TKey, TValue>
    where TKey : IEquatable<TKey>
    where TValue : notnull
{
    /// <summary>
    /// Gets an existing item from cache or creates a new one using the provided factory
    /// </summary>
    /// <param name="key">The cache key</param>
    /// <param name="factory">Factory function to create the value if not found in cache</param>
    /// <param name="context">Context object passed to the factory function</param>
    /// <param name="options">Cache options including expiration settings</param>
    /// <returns>The cached or newly created value</returns>
    ValueTask<TValue> GetOrCreateAsync<TContext>(
        TKey key,
        Func<TKey, TContext, ValueTask<TValue>> factory,
        TContext context,
        CacheOptions options
    )
        where TContext : notnull;

    /// <summary>
    /// Removes an item from the cache
    /// </summary>
    /// <param name="key">The cache key to remove</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    ValueTask RemoveAsync(TKey key);
}
