using System;
using System.Collections.Generic;

namespace Annium.Testing
{
    public static class DictionaryExtensions
    {
        public static TValue At<TKey, TValue>(this IDictionary<TKey, TValue> value, TKey key)
            where TKey : notnull
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            value.ContainsKey(key).IsTrue($"Key `{key}` is not found in dictionary");

            return value[key];
        }

        public static TValue At<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> value, TKey key)
            where TKey : notnull
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            value.ContainsKey(key).IsTrue($"Key `{key}` is not found in dictionary");

            return value[key];
        }

        public static IDictionary<TKey, TValue> Has<TKey, TValue>(this IDictionary<TKey, TValue> value, int count)
            where TKey : notnull
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            var total = value.Count;
            total.Is(count, $"Dictionary expected to have `{count}` items, but has `{total}` items");

            return value;
        }

        public static IReadOnlyDictionary<TKey, TValue> Has<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> value, int count)
            where TKey : notnull
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            var total = value.Count;
            total.Is(count, $"Dictionary expected to have `{count}` items, but has `{total}` items");

            return value;
        }

        public static IDictionary<TKey, TValue> IsEmpty<TKey, TValue>(this IDictionary<TKey, TValue> value)
            where TKey : notnull
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            var total = value.Count;
            total.Is(0, $"Dictionary expected to be empty, but has `{total}` items");

            return value;
        }

        public static IReadOnlyDictionary<TKey, TValue> IsEmpty<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> value)
            where TKey : notnull
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            var total = value.Count;
            total.Is(0, $"Dictionary expected to be empty, but has `{total}` items");

            return value;
        }
    }
}