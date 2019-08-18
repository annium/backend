using System;
using System.Linq;
using System.Reflection;

namespace Annium.Core.Application.Types
{
    public static class TypeExtensions
    {
        // Get implementation of given type, that may contain generic parameters, that implements concrete target type
        public static Type ResolveByImplentations(this Type type, params Type[] targets)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            // if type is defined - no need for resolution
            if (!type.ContainsGenericParameters)
                return type;

            var args = type.ResolveGenericArgumentsByImplentations(targets);
            if (args is null || args.Any(arg => arg is null))
                return null;

            if (type.IsGenericParameter)
                return args.FirstOrDefault();

            var result = type.GetGenericTypeDefinition().MakeGenericType(args);

            return targets.All(target => target.IsAssignableFrom(result)) ? result : null;
        }

        public static Type[] ResolveGenericArgumentsByImplentations(this Type type, params Type[] targets)
        {
            var argsMatrix = targets
                .Select(t =>
                    type.ResolveGenericArgumentsByImplentation(t) ?
                    .Select(a => a.IsGenericParameter ? null : a)
                    .ToArray()
                )
                .OfType<Type[]>()
                .ToList();

            if (argsMatrix.Count == 0)
                return null;

            var args = argsMatrix[0];

            foreach (var argsSet in argsMatrix.Skip(1))
                for (var i = 0; i < args.Length; i++)
                {
                    var arg = args[i];
                    var otherArg = argsSet[i];

                    if (otherArg is null)
                        continue;

                    if (arg is null || arg == otherArg)
                        args[i] = otherArg;
                    else
                        return null;
                }

            return args;
        }

        private static Type[] ResolveGenericArgumentsByImplentation(this Type type, Type target)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            if (target is null)
                throw new ArgumentNullException(nameof(target));

            if (type.IsGenericParameter)
            {
                var attrs = type.GenericParameterAttributes;

                // if reference type required, but target is not class
                if (attrs.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint) && !target.IsClass)
                    return null;

                // if not nullable value type required, but target is not not-nullable value type
                if (attrs.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint) && !target.IsNotNullableValueType())
                    return null;

                var meetsConstraints = type.GetGenericParameterConstraints()
                    .All(constraint => target.GetTargetImplementation(constraint) != null);

                return meetsConstraints ? new [] { target } : null;
            }

            // if type is not generic - return empty array, meaning successful resolution
            if (!type.IsGenericType)
                return Type.EmptyTypes;

            // if type is defined or target is not generic - no need for resolution, just return type's generic args
            if (!type.ContainsGenericParameters)
                return type.GetGenericArguments();

            // if same generic - return target's arguments
            if (type.GetGenericTypeDefinition() == target.GetGenericTypeDefinition())
                return target.GetGenericArguments();

            // type is generic with parameters, target is generic without parameters

            if (target.IsValueType)
                return null;

            if (target.IsClass)
            {
                var baseType = type.BaseType;

                // if no base type or it's not generic - resolution fails, cause types' generic definitions are different
                if (baseType is null || !baseType.IsGenericType)
                    return null;

                if (baseType.GetGenericTypeDefinition() != target.GetGenericTypeDefinition())
                    return resolveBase();

                return buildArgs(baseType);
            }

            if (target.IsInterface)
            {
                // find interface, that is implementation of target's generic definition
                var targetBase = target.GetGenericTypeDefinition();
                var implementation = type.GetOwnInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == targetBase);

                if (implementation != null)
                    return buildArgs(implementation);

                if (type.BaseType is null)
                    return null;

                return resolveBase();
            }

            // otherwise - not implemented or don't know how to resolve
            throw new NotImplementedException($"Can't resolve {type.Name} implementation of {target.Name}");

            Type[] resolveBase()
            {
                var unboundBaseType = type.GetUnboundBaseType();
                var baseArgs = unboundBaseType.ResolveGenericArgumentsByImplentation(target);
                if (baseArgs is null)
                    return null;

                var baseImplementation = type.BaseType.GetGenericTypeDefinition().MakeGenericType(baseArgs);

                return type.ResolveGenericArgumentsByImplentation(baseImplementation);
            }

            Type[] buildArgs(Type sourceType)
            {
                var args = type.GetGenericArguments();
                fillArgs(args, sourceType, target);

                return args;
            }

            void fillArgs(Type[] args, Type sourceType, Type targetType)
            {
                var sourceArgs = sourceType.GetGenericArguments();
                var targetArgs = targetType.GetGenericArguments();

                for (var i = 0; i < sourceArgs.Length; i++)
                {
                    if (sourceArgs[i].IsGenericParameter)
                        args[sourceArgs[i].GenericParameterPosition] = targetArgs[i];
                    else if (sourceArgs[i].ContainsGenericParameters)
                        fillArgs(args, sourceArgs[i], targetArgs[i]);
                }
            }
        }

        // Get base of given concrete type, that implements target type, that may contain generic parameters
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

            if (target.IsValueType)
                return null;

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
                .Select((arg, i) => arg.IsGenericParameter ? genericArgs[i] : arg)
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

        public static bool IsNotNullableValueType(this Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            return type.IsValueType && Nullable.GetUnderlyingType(type) is null;
        }

        public static bool IsNullableValueType(this Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            return type.IsValueType && Nullable.GetUnderlyingType(type) != null;
        }
    }
}