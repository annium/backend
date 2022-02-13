using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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

    public static IReadOnlyDictionary<(TKey Start, TKey End), ISortedListSpan<TKey, TValue>?> GetChunks<TKey, TValue>(
        this SortedList<TKey, TValue> source,
        TKey start,
        TKey end,
        Func<TKey, TKey> nextKey,
        int chunkSize = 1
    )
        where TKey : notnull
    {
        var compare = source.Comparer.Compare;
        if (compare(start, end) > 0)
            throw new ArgumentException($"Sorted list range {start} - {end} is invalid");

        var result = new Dictionary<(TKey Start, TKey End), ISortedListSpan<TKey, TValue>?>();

        var outChunkStart = start;
        var inChunkStart = start;
        var prevKey = start;
        var key = start;
        // detect initial state - in or out of chunk
        var index = source.Keys.IndexOf(key);
        var inSource = index >= 0;
        var size = 0;

        while (compare(key, end) <= 0)
        {
            // get index of current key
            index = source.Keys.IndexOf(key);

            // go to next key if state is same (in or out)
            if (inSource == index >= 0)
            {
                // go to next key
                prevKey = key;
                key = nextKey(key);
                size++;

                continue;
            }

            // state has changed
            inSource = !inSource;

            // if entering chunk
            if (index >= 0)
                inChunkStart = key;

            // if leaving chunk and size is enough for chunk - return new chunk
            if (index == -1 && size >= chunkSize)
            {
                TryAddNullChunk(result, compare, outChunkStart, inChunkStart);
                result[(inChunkStart, prevKey)] = source.GetRange(inChunkStart, prevKey);
                outChunkStart = prevKey;
            }

            // go to next key
            prevKey = key;
            key = nextKey(key);
            size = 1;
        }

        if (inSource && (compare(inChunkStart, start) == 0 || size >= chunkSize))
        {
            TryAddNullChunk(result, compare, outChunkStart, inChunkStart);
            result[(inChunkStart, end)] = source.GetRange(inChunkStart, end);
        }

        if (!inSource)
            TryAddNullChunk(result, compare, outChunkStart, end);

        return result;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void TryAddNullChunk(
            Dictionary<(TKey Start, TKey End), ISortedListSpan<TKey, TValue>?> output,
            Func<TKey, TKey, int> comp,
            TKey from,
            TKey to
        )
        {
            if (comp(from, to) < 0)
                output[(from, to)] = null;
        }
    }
}