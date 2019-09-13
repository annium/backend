using System;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Testing
{
    public static class EnumerableExtensions
    {
        public static void IsEqual<T>(this IEnumerable<T> value, IEnumerable<T> data, string message = null)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            if (data is null)
                throw new ArgumentNullException(nameof(data));

            var count = value.Count();
            if (count != data.Count())
                fail();

            var comparer = EqualityComparer<T>.Default;
            for (var i = 0; i < count; i++)
                if (!comparer.Equals(value.ElementAt(i), data.ElementAt(i)))
                    fail();

            void fail() =>
                throw new AssertionFailedException(message ?? $"{serialize(value)} != {serialize(data)}");

            string serialize(IEnumerable<T> enumerable) =>
                $"[{string.Join(", ", enumerable)}]";
        }

        public static T At<T>(this IEnumerable<T> value, int key)
        {
            var total = value.Count();
            (0 <= key && key < total).IsTrue($"Index `{key}` is out of bounds [0,{total-1}]");

            return value.ElementAt(key);
        }

        public static IEnumerable<T> Has<T>(this IEnumerable<T> value, int count)
        {
            var total = value.Count();
            total.IsEqual(count, $"Enumerable expected to have `{count}` items, but has `{total}` items");

            return value;
        }

        public static IEnumerable<T> IsEmpty<T>(this IEnumerable<T> value)
        {
            var total = value.Count();
            total.IsEqual(0, $"Enumerable expected to be empty, but has `{total}` items");

            return value;
        }
    }
}