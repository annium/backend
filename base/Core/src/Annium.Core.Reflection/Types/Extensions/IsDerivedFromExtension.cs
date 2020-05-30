using System;
using System.Linq;

namespace Annium.Core.Reflection
{
    public static class IsDerivedFromExtension
    {
        public static bool IsDerivedFrom(this Type type, Type target)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            if (target is null)
                throw new ArgumentNullException(nameof(type));

            if (!target.IsGenericTypeDefinition)
                return target.IsAssignableFrom(type);

            if (target.IsClass)
                return type.GetInheritanceChain(false, true).Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == target);

            if (target.IsInterface)
                return type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == target);

            throw new InvalidOperationException($"Type '{target}' cannot be derived from");
        }
    }
}