using System;
using System.Collections.Generic;

namespace Annium.Testing
{
    public static class ArrayExtensions
    {
        public static void IsEqual<T>(this T[] value, T[] data, string message = null)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            if (data is null)
                throw new ArgumentNullException(nameof(data));

            if (value.Length != data.Length)
                fail();

            var comparer = EqualityComparer<T>.Default;
            for (var i = 0; i < value.Length; i++)
                if (!comparer.Equals(value[i], data[i]))
                    fail();

            void fail() =>
                throw new AssertionFailedException(message ?? $"{serialize(value)} != {serialize(data)}");

            string serialize(T[] array) =>
                $"[{string.Join(", ", array)}]";
        }

        public static T At<T>(this T[] value, int key)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            var total = value.Length;
            (0 <= key && key < total).IsTrue($"Index `{key}` is out of bounds [0,{total-1}]");

            return value[key];
        }

        public static T[] Has<T>(this T[] value, int count)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            var total = value.Length;
            total.IsEqual(count, $"Array expected to have `{count}` items, but has `{total}` items");

            return value;
        }

        public static T[] IsEmpty<T>(this T[] value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            var total = value.Length;
            total.IsEqual(0, $"Array expected to be empty, but has `{total}` items");

            return value;
        }
    }
}