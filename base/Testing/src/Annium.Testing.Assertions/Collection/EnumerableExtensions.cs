using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Annium.Core.Mapper;

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
            total.IsEqual(count, Mapper.GetFor(Assembly.GetCallingAssembly()), $"Enumerable expected to have `{count}` items, but has `{total}` items");

            return value;
        }

        public static IEnumerable<T> IsEmpty<T>(this IEnumerable<T> value)
        {
            var total = value.Count();
            total.IsEqual(0, Mapper.GetFor(Assembly.GetCallingAssembly()), $"Enumerable expected to be empty, but has `{total}` items");

            return value;
        }
    }
}