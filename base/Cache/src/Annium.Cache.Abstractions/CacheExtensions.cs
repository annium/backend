using System;
using System.Threading.Tasks;

namespace Annium.Cache.Abstractions;

public static class CacheExtensions
{
    public static ValueTask<TValue> GetOrCreateAsync<TKey, TValue>(
        this ICache<TKey, TValue> cache,
        TKey key,
        Func<TKey, ValueTask<TValue>> factory,
        CacheOptions options
    )
        where TKey : IEquatable<TKey>
        where TValue : notnull
    {
        static ValueTask<TValue> Factory(TKey key, Func<TKey, ValueTask<TValue>> factory) => factory(key);

        return cache.GetOrCreateAsync(key, Factory, factory, options);
    }
}
