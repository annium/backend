using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Core.Primitives;

public static class TypeArrayExtensions
{
    public static bool IsEnumerable(this Type type)
    {
        if (type == typeof(string))
            return false;

        return type.IsArray || type == typeof(IEnumerable) || type.GetInterfaces().Any(x => x == typeof(IEnumerable));
    }

    public static bool IsArrayLike(this Type type) => type.TryGetArrayElementType() is not null;
    public static Type GetArrayElementType(this Type type) => type.TryGetArrayElementType() ?? throw new InvalidOperationException($"Type {type.FriendlyName()} is not array-like type");

    public static Type? TryGetArrayElementType(this Type type)
    {
        if (type == typeof(string))
            return null;

        if (type.IsArray)
            return type.GetElementType();

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            return type.GetGenericArguments()[0];

        var arrayImplementation = type.GetInterfaces()
            .SingleOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        var elementType = arrayImplementation?.GetGenericArguments()[0];

        return elementType;
    }
}