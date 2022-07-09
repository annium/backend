using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Annium.Diagnostics.Debug;

namespace Annium.Core.Internal;

public static class LogExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Trace<T>(
        this T obj,
        string message = "",
        bool withTrace = false,
        [CallerFilePath] string callerFilePath = "",
        [CallerMemberName] string member = "",
        [CallerLineNumber] int line = 0
    )
        where T : class
    {
        var subject = obj is null ? "null" : $"{obj.GetType().FriendlyName()}#{obj.GetId()}";
        var trace = withTrace ? $"{Environment.NewLine}{Environment.StackTrace}" : string.Empty;
        var msg = string.IsNullOrWhiteSpace(message) ? string.Empty : $" >> {message}";
        Log.Trace(subject, $"{msg}{trace}", callerFilePath, member, line);
    }

    private static string FriendlyName(this Type value)
    {
        if (TypeNames.TryGetValue(value, out var name))
            return name;

        if (value.IsGenericParameter || !value.IsGenericType)
            return TypeNames[value] = value.Name;

        if (value.GetGenericTypeDefinition() == typeof(Nullable<>))
            return TypeNames[value] = $"{Nullable.GetUnderlyingType(value)!.FriendlyName()}?";

        name = value.GetGenericTypeDefinition().Name;
        if (name.IndexOf('`') >= 0)
            name = name.Substring(0, name.IndexOf('`'));
        var arguments = value.GetGenericArguments().Select(x => x.FriendlyName()).ToArray();

        return TypeNames.AddOrUpdate(value, $"{name}<{string.Join(", ", arguments)}>", (_, x) => x);
    }

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
}