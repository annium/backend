using System;

namespace Annium.Core.Reflection;

public static class NullableValueTypeExtensions
{
    public static bool IsNotNullableValueType(this Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        return type.IsValueType && Nullable.GetUnderlyingType(type) is null;
    }

    public static bool IsNullableValueType(this Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        return type.IsValueType && Nullable.GetUnderlyingType(type) != null;
    }
}