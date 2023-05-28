using System;
using System.Threading.Tasks;

namespace Annium.Cache.Abstractions;

public interface ICache<TKey, TValue>
    where TKey : notnull
    where TValue : notnull
{
    TValue GetOrCreateAsync(TKey key, Func<TKey, ValueTask<TValue>> factory, CacheOptions options);
    TValue GetOrCreateAsync<TContext>(TKey key, Func<TKey, TContext, ValueTask<TValue>> factory, TContext context, CacheOptions options);
    ValueTask RefreshAsync(TKey key);
    ValueTask RemoveAsync(TKey key);
}