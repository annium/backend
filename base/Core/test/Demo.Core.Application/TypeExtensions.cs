using System;
using System.Linq;
using System.Reflection;

namespace Annium.Core.Application.Types
{
    public static class TypeExtensions
    {
        // ResolveByImplentation - get implementation of given type, that may contain generic parameters, that implements concrete target type
        public static Type ResolveByImplentation(this Type type, Type target)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            if (target is null)
                throw new ArgumentNullException(nameof(target));

            // if type is defined - no need for resolution
            if (!type.ContainsGenericParameters)
                return type;

            if (target.ContainsGenericParameters)
                throw new ArgumentException($"Can't resolve by generic implementation type with parameters");

            // if target is not generic - can't resolve
            if (!target.IsGenericType)
                return null;

            // if same generic - return target
            if (type.GetGenericTypeDefinition() == target.GetGenericTypeDefinition())
                return target;

            if (target.IsClass)
            {
                var baseType = type.BaseType;
                if (baseType is null)
                    return null;

                if (baseType.GetGenericTypeDefinition() != target.GetGenericTypeDefinition())
                {
                    var unboundBaseType = type.GetUnboundBaseType();
                    var baseImplementation = unboundBaseType.ResolveByImplentation(target);

                    return baseImplementation is null ? null : type.ResolveByImplentation(baseImplementation);
                }

                return buildImplementation(baseType);
            }

            if (target.IsInterface)
            {
                // find interface, that is implementation of target's generic definition
                var targetBase = target.GetGenericTypeDefinition();
                var implementation = type.GetOwnInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == targetBase);

                if (implementation is null)
                {
                    if (type.BaseType is null)
                        return null;

                    var unboundBaseType = type.GetUnboundBaseType();
                    var baseImplementation = unboundBaseType.ResolveByImplentation(target);

                    return baseImplementation is null ? null : type.ResolveByImplentation(baseImplementation);
                }

                return buildImplementation(implementation);
            }

            // otherwise - not implemented or don't know how to resolve
            throw new NotImplementedException($"Can't resolve {type.Name} implementation of {target.Name}");

            Type buildImplementation(Type implementation)
            {
                // restore args from interface
                var args = type.GetGenericArguments();
                var baseArgs = implementation.GetGenericArguments();
                var targetArgs = target.GetGenericArguments();

                for (var i = 0; i < baseArgs.Length; i++)
                    if (baseArgs[i].IsGenericTypeParameter)
                        args[baseArgs[i].GenericParameterPosition] = targetArgs[i];

                if (args.Any(arg => arg.IsGenericParameter))
                    return null;

                var result = type.GetGenericTypeDefinition().MakeGenericType(args);

                return target.IsAssignableFrom(result) ? result : null;
            }
        }

        // GetTargetImplementation - get base of given concrete type, that implements target type, that may contain generic parameters
        public static Type GetTargetImplementation(this Type type, Type target)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            if (target is null)
                throw new ArgumentNullException(nameof(target));

            if (type.ContainsGenericParameters)
                throw new ArgumentException($"Can't resolve implementation of generic type with parameters");

            if (target.IsAssignableFrom(type))
                return target;

            if (target.IsGenericParameter)
            {
                var attrs = target.GenericParameterAttributes;

                // if reference type required, but target is not class
                if (attrs.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint) && !target.IsClass)
                    return null;

                // if not nullable value type required, but target is not value type or is nullable value type
                if (attrs.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint) && (!target.IsValueType || Nullable.GetUnderlyingType(target) != null))
                    return null;

                var meetsConstraints = target.GetGenericParameterConstraints()
                    .All(constraint => type.GetTargetImplementation(constraint) != null);

                return meetsConstraints ? type : null;
            }

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

            // otherwise - not implemented or don't know how to resolve
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
                            targetArg.ContainsGenericParameters ? implementationArg.GetTargetImplementation(targetArg) : targetArg
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

        public static Type GetUnboundBaseType(this Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            var baseType = type.BaseType;
            if (baseType == null)
                return null;

            if (!baseType.ContainsGenericParameters)
                return baseType;

            var genericArgs = baseType.GetGenericTypeDefinition().GetGenericArguments();
            var unboundBaseArgs = baseType.GetGenericArguments()
                .Select((arg, i) => arg.IsGenericTypeParameter ? genericArgs[i] : arg)
                .ToArray();

            return baseType.GetGenericTypeDefinition().MakeGenericType(unboundBaseArgs);
        }

        public static Type[] GetOwnInterfaces(this Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            if (type.BaseType == null)
                return type.GetInterfaces();

            var interfaces = type.GetInterfaces();
            var baseInterfaces = type.BaseType.GetInterfaces();

            return interfaces
                .Where(i => !baseInterfaces.Contains(i))
                .ToArray();
        }
    }
}