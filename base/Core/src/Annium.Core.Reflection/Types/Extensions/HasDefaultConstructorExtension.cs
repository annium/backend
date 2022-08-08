using System;
using System.Reflection;

namespace Annium.Core.Reflection;

public static class HasDefaultConstructorExtension
{
    private static readonly BindingFlags BindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    public static bool HasDefaultConstructor(this Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        if (type.IsClass)
            return type.GetConstructor(BindingFlags, Type.EmptyTypes) != null;

        if (type.IsValueType)
            return type.GetConstructors(BindingFlags).Length == 0 || type.GetConstructor(BindingFlags, Type.EmptyTypes) != null;

        throw new ArgumentException($"{type} is not constructable");
    }
}