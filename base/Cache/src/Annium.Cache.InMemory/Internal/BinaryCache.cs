using System;
using System.Threading.Tasks;
using Annium.Cache.Abstractions;

namespace Annium.Cache.InMemory.Internal;

internal class BinaryCache<TKey, TValue> : ICache<TKey, ReadOnlyMemory<byte>>
    where TKey : notnull
{
    public ReadOnlyMemory<byte> GetOrCreateAsync(TKey key, Func<TKey, ValueTask<ReadOnlyMemory<byte>>> factory, CacheOptions options)
    {
        throw new NotImplementedException();
    }

    public ReadOnlyMemory<byte> GetOrCreateAsync<TContext>(TKey key, Func<TKey, TContext, ValueTask<ReadOnlyMemory<byte>>> factory, TContext context, CacheOptions options)
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