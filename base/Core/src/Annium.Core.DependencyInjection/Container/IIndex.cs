using System.Collections.Generic;

namespace Annium.Core.DependencyInjection
{
    public interface IIndex<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
        where TKey : notnull
        where TValue : notnull
    {
    }
}