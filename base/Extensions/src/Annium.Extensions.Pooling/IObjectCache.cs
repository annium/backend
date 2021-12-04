using System;
using System.Threading.Tasks;

namespace Annium.Extensions.Pooling;

public interface IObjectCache<TKey, TValue> : IAsyncDisposable
    where TKey : notnull
    where TValue : notnull
{
    Task<ICacheReference<TValue>> GetAsync(TKey key);
}