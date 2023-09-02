using System;
using System.Runtime.CompilerServices;

namespace Annium;

public static class NullableExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T NotNull<T>(
        this T? value,
        [CallerArgumentExpression("value")] string expression = ""
    )
        where T : class
    {
        if (value is not null)
            return value;

        throw Exception(expression);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T NotNull<T>(
        this T? value,
        [CallerArgumentExpression("value")] string expression = ""
    )
        where T : struct
    {
        if (value.HasValue)
            return value.Value;

        throw Exception(expression);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static NullReferenceException Exception(string expression) =>
        new NullReferenceException($"{expression} is null");
}