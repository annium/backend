using System;
using System.Threading.Tasks;

namespace Annium.Cache.Abstractions;

public interface ICache<TKey, TValue>
    where TKey : IEquatable<TKey>
    where TValue : notnull
{
    ValueTask<TValue> GetOrCreateAsync<TContext>(
        TKey key,
        Func<TKey, TContext, ValueTask<TValue>> factory,
        TContext context,
        CacheOptions options
    )
        where TContext : notnull;

    ValueTask RemoveAsync(TKey key);
}
