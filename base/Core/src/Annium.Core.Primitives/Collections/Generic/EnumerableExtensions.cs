using System;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Core.Primitives
{
    public static class EnumerableExtensions
    {
        private static readonly Random Random = new();
        public static string Join(this IEnumerable<string> src, string separator = "") => string.Join(separator, src);
        public static string Join(this IEnumerable<string> src, char separator = ' ') => string.Join(separator, src);

        public static IEnumerable<T> Yield<T>(this T src)
        {
            if (src is null)
                yield break;

            yield return src;
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> src)
        {
            return src.OrderBy(_ => Random.Next(0, 1) == 1);
        }
    }
}