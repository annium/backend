using System;
using System.Linq;

namespace Annium.Core.Reflection
{
    public static class ResolveByImplementationExtension
    {
        // Get implementation of given type, that may contain generic parameters, that implements concrete target type
        public static Type? ResolveByImplementation(this Type type, Type target)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            // if type is defined - no need for resolution
            if (!type.ContainsGenericParameters)
                // TODO: fix
                return type;

            var args = type.ResolveGenericArgumentsByImplentation(target);
            if (args is null || args.Any(arg => arg is null))
                return null;

            if (type.IsGenericParameter)
                return args.FirstOrDefault();

            if (!type.GetGenericTypeDefinition().TryMakeGenericType(out var result, args))
                return null;

            return target.IsAssignableFrom(result) ? result : null;
        }
    }
}