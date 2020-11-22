using System;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Core.DependencyInjection.Obsolete
{
    public static class RegistrationBuilderExtensions
    {
        [Obsolete]
        public static IRegistrationBuilder AssignableTo<T>(this IRegistrationBuilder builder)
            => builder.Where(x => x.IsDerivedFrom(typeof(T)));

        [Obsolete]
        public static IRegistrationBuilder AssignableTo(this IRegistrationBuilder builder, Type baseType)
            => builder.Where(x => x.IsDerivedFrom(baseType));

        [Obsolete]
        public static IRegistrationBuilder StartingWith(this IRegistrationBuilder builder, string prefix)
            => builder.Where(x => x.Name.StartsWith(prefix));

        [Obsolete]
        public static IRegistrationBuilder EndingWith(this IRegistrationBuilder builder, string suffix)
            => builder.Where(x => x.Name.EndsWith(suffix));
    }

    internal static class IsDerivedFromExtension
    {
        public static bool IsDerivedFrom(this Type type, Type target)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            if (target is null)
                throw new ArgumentNullException(nameof(type));

            if (!target.IsGenericTypeDefinition)
                return type != target && target.IsAssignableFrom(type);

            if (target.IsClass)
                return type.GetInheritanceChain(false, true).Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == target);

            if (target.IsInterface)
                return type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == target);

            throw new InvalidOperationException($"Type '{target}' cannot be derived from");
        }

        public static Type[] GetInheritanceChain(
            this Type type,
            bool self = false,
            bool root = false
        )
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            var chain = new HashSet<Type>();
            if (self)
                chain.Add(type);

            if (type.IsValueType || type == typeof(ValueType))
            {
                if (root)
                    chain.Add(typeof(ValueType));

                return chain.ToArray();
            }

            if (type.IsClass)
            {
                if (type.BaseType != null)
                    while (type!.BaseType != typeof(object))
                    {
                        chain.Add(type!.BaseType!);
                        type = type.BaseType!;
                    }

                if (root)
                    chain.Add(typeof(object));

                return chain.ToArray();
            }

            return Array.Empty<Type>();
        }
    }
}