using System.Collections.Generic;
using System.Linq;

namespace Annium.Testing
{
    public static class EnumerableExtensions
    {
        public static T At<T>(this IEnumerable<T> value, int key)
        {
            var total = value.Count();
            (0 <= key && key < total).IsTrue($"Index `{key}` is out of bounds [0,{total - 1}]");

            return value.ElementAt(key);
        }

        public static IEnumerable<T> Has<T>(this IEnumerable<T> value, int count)
        {
            var total = value.Count();
            total.Is(count, $"Enumerable expected to have `{count}` items, but has `{total}` items");

            return value;
        }

        public static IEnumerable<T> IsEmpty<T>(this IEnumerable<T> value)
        {
            var total = value.Count();
            total.Is(0, $"Enumerable expected to be empty, but has `{total}` items");

            return value;
        }
    }
}