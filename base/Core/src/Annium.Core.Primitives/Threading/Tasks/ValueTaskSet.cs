using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Annium.Core.Primitives.Threading.Tasks;

public static class ValueTaskSet
{
    public static async ValueTask<T[]> WhenAll<T>(IReadOnlyList<ValueTask<T>> tasks)
    {
        List<Exception>? exceptions = null;

        var results = new T[tasks.Count];
        for (var i = 0; i < tasks.Count; i++)
            try
            {
                results[i] = await tasks[i].ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                exceptions ??= new List<Exception>(tasks.Count);
                exceptions.Add(ex);
            }

        return exceptions is null
            ? results
            : throw new AggregateException(exceptions);
    }

    public static ValueTask<T[]> WhenAll<T>(IEnumerable<ValueTask<T>> tasks) =>
        WhenAll(tasks.ToList());

    public static ValueTask<T[]> WhenAll<T>(params ValueTask<T>[] tasks) =>
        WhenAll(tasks as IReadOnlyList<ValueTask<T>>);
}