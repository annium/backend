using System;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Core.Primitives
{
    public static class DictionaryExtensions
    {
        public static void RemoveAll<TKey, TValue>(
            this IDictionary<TKey, TValue> src,
            Func<TKey, bool> predicate
        )
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
}