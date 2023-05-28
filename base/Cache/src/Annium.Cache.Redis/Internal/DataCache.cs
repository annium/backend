using System;
using System.Threading.Tasks;
using Annium.Cache.Abstractions;

namespace Annium.Cache.Redis.Internal;

internal class DataCache<TKey, TValue> : ICache<TKey, TValue>
    where TKey : notnull
    where TValue : notnull
{
    public TValue GetOrCreateAsync(TKey key, Func<TKey, ValueTask<TValue>> factory, CacheOptions options)
    {
        throw new NotImplementedException();
    }

    public TValue GetOrCreateAsync<TContext>(TKey key, Func<TKey, TContext, ValueTask<TValue>> factory, TContext context, CacheOptions options)
    {
        throw new NotImplementedException();
    }

    public ValueTask RefreshAsync(TKey key)
    {
        throw new NotImplementedException();
    }

    public ValueTask RemoveAsync(TKey key)
    {
        throw new NotImplementedException();
    }
}