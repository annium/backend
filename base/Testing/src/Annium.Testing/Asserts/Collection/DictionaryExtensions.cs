using System;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Testing
{
    public static class DictionaryExtensions
    {
        public static void IsEqual<TKey, TValue>(this IDictionary<TKey, TValue> value, IDictionary<TKey, TValue> data, string message = null)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            if (data is null)
                throw new ArgumentNullException(nameof(data));

            if (value.Count != data.Count)
                fail();

            var comparer = EqualityComparer<TValue>.Default;

            foreach (var key in value.Keys)
                if (!data.ContainsKey(key) || !comparer.Equals(value[key], data[key]))
                    fail();

            void fail() =>
                throw new AssertionFailedException(message ?? $"{serialize(value)} != {serialize(data)}");

            string serialize(IDictionary<TKey, TValue> dictionary) =>
                $"{{{string.Join(", ", dictionary.Select(p => $@"""{p.Key}"": ""{p.Value}"""))}}}";
        }

        public static void IsEqual<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> value, IReadOnlyDictionary<TKey, TValue> data, string message = null)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            if (data is null)
                throw new ArgumentNullException(nameof(data));

            if (value.Count != data.Count)
                fail();

            var comparer = EqualityComparer<TValue>.Default;

            foreach (var key in value.Keys)
                if (!data.ContainsKey(key) || !comparer.Equals(value[key], data[key]))
                    fail();

            void fail() =>
                throw new AssertionFailedException(message ?? $"{serialize(value)} != {serialize(data)}");

            string serialize(IReadOnlyDictionary<TKey, TValue> dictionary) =>
                $"{{{string.Join(", ", dictionary.Select(p => $@"""{p.Key}"": ""{p.Value}"""))}}}";
        }

        public static TValue At<TKey, TValue>(this IDictionary<TKey, TValue> value, TKey key)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            value.ContainsKey(key).IsTrue($"Key `{key}` is not found in dictionary");

            return value[key];
        }

        public static TValue At<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> value, TKey key)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            value.ContainsKey(key).IsTrue($"Key `{key}` is not found in dictionary");

            return value[key];
        }

        public static IDictionary<TKey, TValue> Has<TKey, TValue>(this IDictionary<TKey, TValue> value, int count)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            var total = value.Count;
            total.IsEqual(count, $"Dictionary expected to have `{count}` items, but has `{total}` items");

            return value;
        }

        public static IReadOnlyDictionary<TKey, TValue> Has<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> value, int count)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            var total = value.Count;
            total.IsEqual(count, $"Dictionary expected to have `{count}` items, but has `{total}` items");

            return value;
        }

        public static IDictionary<TKey, TValue> IsEmpty<TKey, TValue>(this IDictionary<TKey, TValue> value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            var total = value.Count;
            total.IsEqual(0, $"Dictionary expected to be empty, but has `{total}` items");

            return value;
        }

        public static IReadOnlyDictionary<TKey, TValue> IsEmpty<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            var total = value.Count;
            total.IsEqual(0, $"Dictionary expected to be empty, but has `{total}` items");

            return value;
        }
    }
}