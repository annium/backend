using System.Collections.Generic;

namespace Annium.Collections.Generic;

public static class ReadOnlyListExtensions
{
    public static IListSpan<T> ToListSpan<T>(this IReadOnlyList<T> items, int start, int count) =>
        new ListSpan<T>(items, start, count);
}