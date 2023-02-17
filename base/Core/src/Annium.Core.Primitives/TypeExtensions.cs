using System;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace Annium.Core.Primitives;

public static class TypeExtensions
{
    public static bool IsEnumerable(this Type type)
    {
        if (type == typeof(string))
            return false;

        return type.IsArray || type == typeof(IEnumerable) || type.GetInterfaces().Any(x => x == typeof(IEnumerable));
    }

    public static object? DefaultValue(this Type type)
    {
        return type.GetTypeInfo().IsValueType ? Activator.CreateInstance(type) : null;
    }
}