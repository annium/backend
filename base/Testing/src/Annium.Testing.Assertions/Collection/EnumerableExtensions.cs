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
        var count = value.Count();
        (0 <= key && key < count).IsTrue($"{valueEx}[{key.Wrap(keyEx)}] is out of bounds [0,{count - 1}]");

        return value.ElementAt(key);
    }

    public static IEnumerable<T> Has<T>(
        this IEnumerable<T> value,
        int count,
        [CallerArgumentExpression("value")] string valueEx = default!,
        [CallerArgumentExpression("count")] string countEx = default!
    )
    {
        var total = value.Count();
        total.Is(count, $"{valueEx} count `{total}` != `{count.Wrap(countEx)}`");

        return value;
    }

    public static IEnumerable<T> IsEmpty<T>(
        this IEnumerable<T> value,
        [CallerArgumentExpression("value")] string valueEx = default!
    )
    {
        var total = value.Count();
        total.Is(0, $"{valueEx} expected to be empty, but has `{total}` items");

        return value;
    }

    public static IEnumerable<T> IsNotEmpty<T>(
        this IEnumerable<T> value,
        [CallerArgumentExpression("value")] string valueEx = default!
    )
    {
        value.Any().IsTrue($"{valueEx} expected to be not empty");

        return value;
    }
}