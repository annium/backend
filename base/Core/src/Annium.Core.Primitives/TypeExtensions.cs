using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Annium.Core.Primitives
{
    public static class TypeExtensions
    {
        private static readonly ConcurrentDictionary<Type, string> TypeNames = new(new Dictionary<Type, string>()
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

        public static string FriendlyName(this Type value)
        {
            if (TypeNames.TryGetValue(value, out var name))
                return name;

            if (value.IsGenericParameter || !value.IsGenericType)
                return TypeNames[value] = value.Name;

            if (value.GetGenericTypeDefinition() == typeof(Nullable<>))
                return TypeNames[value] = $"{Nullable.GetUnderlyingType(value)!.FriendlyName()}?";

            name = value.GetGenericTypeDefinition().Name;
            name = name.Substring(0, name.IndexOf('`'));
            var arguments = value.GetGenericArguments().Select(x => x.FriendlyName()).ToArray();

            return TypeNames.AddOrUpdate(value, $"{name}<{string.Join(", ", arguments)}>", (_, x) => x);
        }

        public static bool IsEnumerable(this Type type)
        {
            if (type == typeof(string))
                return false;

            return type.IsArray
                || type == typeof(IEnumerable)
                || type.GetInterfaces().Any(x => x == typeof(IEnumerable));
        }

        public static object? DefaultValue(this Type type)
        {
            if (type.GetTypeInfo().IsValueType)
                return Activator.CreateInstance(type);

            return null;
        }
    }
}