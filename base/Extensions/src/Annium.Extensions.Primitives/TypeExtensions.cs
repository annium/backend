using System;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Extensions.Primitives
{
    public static class TypeExtensions
    {
        private static readonly IReadOnlyDictionary<Type, string> BaseTypeNames = new Dictionary<Type, string>
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
        };

        public static string FriendlyName(this Type value)
        {
            if (BaseTypeNames.TryGetValue(value, out var name))
                return name;

            if (value.IsGenericParameter || !value.IsGenericType)
                return value.Name;

            if (value.GetGenericTypeDefinition() == typeof(Nullable<>))
                return $"{Nullable.GetUnderlyingType(value)!.FriendlyName()}?";

            name = value.GetGenericTypeDefinition().Name;
            name = name.Substring(0, name.IndexOf('`'));
            var arguments = value.GetGenericArguments().Select(x => x.FriendlyName()).ToArray();

            return $"{name}<{string.Join(", ", arguments)}>";
        }
    }
}