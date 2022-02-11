using System;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Collections.Generic;

public static class EnumerableExtensions
{
    public static SortedList<TKey, T> ToSortedList<TKey, T>(this IEnumerable<T> items, Func<T, TKey> getKey)
        where TKey : notnull
    {
        return new SortedList<TKey, T>(items.ToDictionary(getKey, x => x));
    }
}