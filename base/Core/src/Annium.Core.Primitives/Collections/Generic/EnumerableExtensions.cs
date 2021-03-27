using System.Collections.Generic;

namespace Annium.Core.Primitives.Collections.Generic
{
    public static class EnumerableExtensions
    {
        public static string Join(this IEnumerable<string> src, string separator = "") => string.Join(separator, src);
        public static string Join(this IEnumerable<string> src, char separator = ' ') => string.Join(separator, src);

        public static IEnumerable<T> Yield<T>(this T src)
        {
            if (src is null)
                yield break;

            yield return src;
        }
    }
}