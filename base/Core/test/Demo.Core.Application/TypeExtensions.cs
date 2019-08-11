using System;
using System.Linq;

namespace Annium.Core.Application.Types
{
    public static class TypeExtensions
    {
        public static Type GetImplementationOf(this Type type, Type target)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(target));
            if (target is null)
                throw new ArgumentNullException(nameof(target));

            if (type.ContainsGenericParameters)
                throw new ArgumentException($"Can't resolve implementation of generic type with parameters");

            if (target.IsAssignableFrom(type))
                return target;

            if (target.IsClass)
            {
                if (!target.ContainsGenericParameters)
                    return null;

                var targetBase = target.GetGenericTypeDefinition();
                var implementation = type;
                while (implementation != null)
                {
                    if (implementation.IsGenericType && implementation.GetGenericTypeDefinition() == targetBase)
                        break;

                    implementation = implementation.BaseType;
                }

                return buildImplementation(implementation);
            }

            if (target.IsInterface)
            {
                if (!target.ContainsGenericParameters)
                    return null;

                var targetBase = target.GetGenericTypeDefinition();
                var implementation = type.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == targetBase);

                return buildImplementation(implementation);
            }

            // for others - not implemented for now
            throw new NotImplementedException($"Can't resolve {type.Name} implementation of {target.Name}");

            Type buildImplementation(Type implementation)
            {
                if (implementation is null)
                    return null;

                try
                {
                    if (target.IsGenericTypeDefinition)
                        return target.MakeGenericType(implementation.GetGenericArguments());

                    var targetArgs = target.GenericTypeArguments;
                    var implementationArgs = implementation.GetGenericArguments();
                    var args = targetArgs
                        .Zip(implementationArgs, (targetArg, implementationArg) =>
                            targetArg.ContainsGenericParameters ? implementationArg.GetImplementationOf(targetArg) : targetArg
                        )
                        .ToArray();
                    if (args.Any(arg => arg is null))
                        return null;

                    var result = target.GetGenericTypeDefinition().MakeGenericType(args);

                    return result.IsAssignableFrom(type) ? result : null;
                }
                catch
                {
                    return null;
                }

            };
        }
    }
}