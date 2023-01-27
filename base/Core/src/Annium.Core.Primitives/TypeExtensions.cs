using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Annium.Core.Primitives;

public static class TypeExtensions
{
    private static readonly ConcurrentDictionary<Type, string> TypeNames = new(new Dictionary<Type, string>
        {
            { typeof(int), "int" },
            { typeof(uint), "uint" },
            { typeof(long), "long" },
            { typeof(ulong), "ulong" },
            { typeof(short), "short" },
            { typeof(ushort), "ushort" },
            { typeof(byte), "byte" },
            { typeof(sbyte), "sbyte" },
            { typeof(bool), "bool" },
            { typeof(float), "float" },
            { typeof(double), "double" },
            { typeof(decimal), "decimal" },
            { typeof(char), "char" },
            { typeof(string), "string" },
            { typeof(object), "object" },
            { typeof(void), "void" },
        }
    );

    public static string FriendlyName(this Type value) => TypeNames.GetOrAdd(value, BuildFriendlyName);

    private static string BuildFriendlyName(Type type)
    {
        if (type.IsGenericParameter || !type.IsGenericType)
            return type.Name;

        if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
            return $"{Nullable.GetUnderlyingType(type)!.FriendlyName()}?";

        var name = type.GetGenericTypeDefinition().Name;
        var tickIndex = name.IndexOf('`');
        if (tickIndex >= 0)
            name = name[..tickIndex];
        var arguments = type.GetGenericArguments().Select(x => x.FriendlyName()).ToArray();

        return $"{name}<{string.Join(", ", arguments)}>";
    }

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