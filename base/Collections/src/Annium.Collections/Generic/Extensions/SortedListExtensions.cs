using System.Collections.Generic;

namespace Annium.Collections.Generic;

public static class SortedListExtensions
{
    public static ISortedListSpan<TKey, TValue> ToListSpan<TKey, TValue>(
        this SortedList<TKey, TValue> items,
        int start,
        int count
    )
        where TKey : notnull =>
        new SortedListSpan<TKey, TValue>(items, start, count);

    public static ISortedListSpan<TKey, TValue>? GetRange<TKey, TValue>(
        this SortedList<TKey, TValue> source,
        TKey start,
        TKey end
    )
        where TKey : notnull
    {
        var startIndex = source.Keys.IndexOf(start);
        var endIndex = source.Keys.IndexOf(end);

        if (startIndex < 0 || endIndex < 0)
            return null;

        return new SortedListSpan<TKey, TValue>(source, startIndex, endIndex - startIndex + 1);
    }

    // public IEnumerable<IReadOnlyList<T>> GetChunks<TKey, TValue>(
    //     List<T> source, long start, long end)
    //     where T : ITimeSeriesModel
    // {
    //     throw new NotImplementedException();
    // }
}