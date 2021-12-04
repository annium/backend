using System;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Core.Primitives.Collections.Generic;

public static class DictionaryExtensions
{
    public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(
        this IEnumerable<KeyValuePair<TKey, TValue>> src
    )
        where TKey : notnull
    {
        return src.ToDictionary(x => x.Key, x => x.Value);
    }

    public static void RemoveAll<TKey, TValue>(
        this IDictionary<TKey, TValue> src,
        Func<TKey, bool> predicate
    )
        where TKey : notnull
    {
        foreach (var key in src.Keys.Where(predicate).ToArray())
            src.Remove(key);
    }

    public static void RemoveAll<TKey, TValue>(
        this IDictionary<TKey, TValue> src,
        Func<TValue, bool> predicate
    )
    {
        foreach (var item in src.Where(x => predicate(x.Value)).ToArray())
            src.Remove(item);
    }

    public static void RemoveAll<TKey, TValue>(
        this IDictionary<TKey, TValue> src,
        Func<TKey, TValue, bool> predicate
    )
    {
        foreach (var item in src.Where(x => predicate(x.Key, x.Value)).ToArray())
            src.Remove(item);
    }
}