using System.Collections.Generic;

namespace Annium.Testing
{
    public static class DictionaryExtensions
    {
        public static TValue At<TKey, TValue>(this IDictionary<TKey, TValue> value, TKey key)
        {
            value.ContainsKey(key).IsTrue($"Key `{key}` is not found in dictionary");

            return value[key];
        }

        public static TValue At<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> value, TKey key)
        {
            value.ContainsKey(key).IsTrue($"Key `{key}` is not found in dictionary");

            return value[key];
        }

        public static IDictionary<TKey, TValue> Has<TKey, TValue>(this IDictionary<TKey, TValue> value, int count)
        {
            var total = value.Count;
            total.IsEqual(count, $"Dictionary expected to have `{count}` items, but has `{total}` items");

            return value;
        }

        public static IReadOnlyDictionary<TKey, TValue> Has<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> value, int count)
        {
            var total = value.Count;
            total.IsEqual(count, $"Dictionary expected to have `{count}` items, but has `{total}` items");

            return value;
        }

        public static IDictionary<TKey, TValue> IsEmpty<TKey, TValue>(this IDictionary<TKey, TValue> value)
        {
            var total = value.Count;
            total.IsEqual(0, $"Dictionary expected to be empty, but has `{total}` items");

            return value;
        }

        public static IReadOnlyDictionary<TKey, TValue> IsEmpty<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> value)
        {
            var total = value.Count;
            total.IsEqual(0, $"Dictionary expected to be empty, but has `{total}` items");

            return value;
        }
    }
}