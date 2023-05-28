using System;
using System.Threading.Tasks;
using Annium.Cache.Abstractions;

namespace Annium.Cache.Redis.Internal;

internal class StringCache<TKey> : ICache<TKey, string>
    where TKey : notnull
{
    public string GetOrCreateAsync(TKey key, Func<TKey, ValueTask<string>> factory, CacheOptions options)
    {
        throw new NotImplementedException();
    }

    public string GetOrCreateAsync<TContext>(TKey key, Func<TKey, TContext, ValueTask<string>> factory, TContext context, CacheOptions options)
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