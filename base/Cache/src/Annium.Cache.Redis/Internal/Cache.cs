using System;
using System.Threading.Tasks;
using Annium.Cache.Abstractions;

namespace Annium.Cache.Redis.Internal;

internal class Cache<TKey, TValue> : ICache<TKey, TValue>
    where TKey : IEquatable<TKey>
    where TValue : notnull
{
    public ValueTask<TValue> GetOrCreateAsync<TContext>(TKey key, Func<TKey, TContext, ValueTask<TValue>> factory, TContext context, CacheOptions options) where TContext : notnull
    {
        throw new NotImplementedException();
    }

    public ValueTask RemoveAsync(TKey key)
    {
        throw new NotImplementedException();
    }
}