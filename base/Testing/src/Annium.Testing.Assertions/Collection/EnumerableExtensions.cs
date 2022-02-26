using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Annium.Testing.Assertions.Internal;

namespace Annium.Testing;

public static class EnumerableExtensions
{
    public static T At<T>(
        this IEnumerable<T> value,
        int key,
        [CallerArgumentExpression("value")] string valueEx = default!,
        [CallerArgumentExpression("key")] string keyEx = default!
    )
    {
        var data = value.ToArray();
        var total = data.Length;
        (0 <= key && key < total).IsTrue($"{valueEx}[{key.Wrap(keyEx)}] is out of bounds [0,{total - 1}]");

        return data[key];
    }

    public static IEnumerable<T> Has<T>(
        this IEnumerable<T> value,
        int count,
        [CallerArgumentExpression("value")] string valueEx = default!,
        [CallerArgumentExpression("count")] string countEx = default!
    )
    {
        var data = value.ToArray();
        var total = data.Length;
        total.Is(count, $"{valueEx} count `{total}` != `{count.Wrap(countEx)}`");

        return data;
    }

    public static IEnumerable<T> IsEmpty<T>(
        this IEnumerable<T> value,
        [CallerArgumentExpression("value")] string valueEx = default!
    )
    {
        var data = value.ToArray();
        var total = data.Length;
        total.Is(0, $"{valueEx} expected to be empty, but has `{total}` items");

        return data;
    }
}