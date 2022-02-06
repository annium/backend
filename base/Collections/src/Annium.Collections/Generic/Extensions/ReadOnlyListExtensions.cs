using System.Collections.Generic;

namespace Annium.Collections.Generic;

public static class ReadOnlyListExtensions
{
    public static IndexedSpan<T> ToReadOnlyIndexedSpan<T>(this IReadOnlyList<T> items, int start, int count) =>
        new(items, start, count);
}