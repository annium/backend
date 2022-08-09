using System;
using System.Linq;
using System.Reflection;

namespace Annium.Core.Reflection;

public static class ResolveGenericArgumentsByImplementationExtension
{
    public static Type[]? ResolveGenericArgumentsByImplementation(this Type type, Type target)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        if (target is null)
            throw new ArgumentNullException(nameof(target));

        if (!type.IsGenericParameter)
        {
            // if type is not generic - return empty array, meaning successful resolution
            if (!type.IsGenericType)
                return type.GetTargetImplementation(target) is null ? null : Type.EmptyTypes;

            // if type is defined - return it's arguments
            if (!type.ContainsGenericParameters)
                return type.GetTargetImplementation(target) is null ? null : type.GetGenericArguments();
        }

        // if target is not generic - return type's generic arguments, if target is implemented
        if (!type.IsGenericParameter && !target.IsGenericParameter && !target.IsGenericType)
            return target.IsAssignableFrom(type) ? type.GetGenericArguments() : null;

        // as of here:
        // - type is open generic type with generic parameters
        // - target is open/defined generic type with/without generic parameters

        if (type.IsGenericParameter)
            return type.ResolveGenericParameterArgumentsByTarget(target);
        if (type.IsClass)
            return type.ResolveClassArgumentsByTarget(target);
        if (type.IsValueType)
            return type.ResolveStructArgumentsByByTarget(target);
        if (type.IsInterface)
            return type.ResolveInterfaceArgumentsByTarget(target);

        // otherwise - not implemented or don't know how to resolve
        throw GetException(type, target);
    }

    private static Type[]? ResolveGenericParameterArgumentsByTarget(this Type type, Type target)
    {
        if (target.IsGenericParameter)
            return type.ResolveGenericParameterArgumentsByGenericParameter(target);
        if (target.IsClass)
            return type.ResolveGenericParameterArgumentsByClass(target);
        if (target.IsValueType)
            return type.ResolveGenericParameterArgumentsByStruct(target);
        if (target.IsInterface)
            return type.ResolveGenericParameterArgumentsByInterface(target);

        throw GetException(type, target);
    }

    private static Type[]? ResolveClassArgumentsByTarget(this Type type, Type target)
    {
        if (target.IsGenericParameter)
            return type.ResolveClassArgumentsByGenericParameter(target);
        if (target.IsClass)
            return type.ResolveClassArgumentsByClass(target);
        if (target.IsValueType)
            return null;
        if (target.IsInterface)
            return type.ResolveClassArgumentsByInterface(target);

        throw GetException(type, target);
    }

    private static Type[]? ResolveStructArgumentsByByTarget(this Type type, Type target)
    {
        if (target.IsGenericParameter)
            return type.ResolveStructArgumentsByGenericParameter(target);
        if (target.IsClass)
            return null;
        if (target.IsValueType)
            return type.ResolveStructArgumentsByStruct(target);
        if (target.IsInterface)
            return type.ResolveStructArgumentsByInterface(target);

        throw GetException(type, target);
    }

    private static Type[]? ResolveInterfaceArgumentsByTarget(this Type type, Type target)
    {
        if (target.IsGenericParameter)
            return type.ResolveInterfaceArgumentsByGenericParameter(target);
        if (target.IsClass)
            return null;
        if (target.IsValueType)
            return null;
        if (target.IsInterface)
            return type.ResolveInterfaceArgumentsByInterface(target);

        throw GetException(type, target);
    }

    private static Type[]? ResolveGenericParameterArgumentsByGenericParameter(this Type type, Type target)
    {
        var typeAttrs = type.GenericParameterAttributes;
        var targetAttrs = target.GenericParameterAttributes;

        // if reference type constraint is not presented
        if (targetAttrs.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint) &&
            !typeAttrs.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
            return null;

        // if not nullable value type constraint is not presented
        if (targetAttrs.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint) &&
            !typeAttrs.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
            return null;

        // if default parameter constraint is not presented
        if (targetAttrs.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint) &&
            !typeAttrs.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
            return null;

        // ensure all parameter constraints are implemented
        var meetsConstraints = target.GetGenericParameterConstraints()
            .All(targetConstraint => type.GetGenericParameterConstraints()
                .Any(typeConstraint => typeConstraint.ResolveGenericArgumentsByImplementation(targetConstraint) != null)
            );

        return meetsConstraints ? new[] { type } : null;
    }

    private static Type[]? ResolveGenericParameterArgumentsByClass(this Type type, Type target)
    {
        var attrs = type.GenericParameterAttributes;

        // if not nullable value type constraint is not presented
        if (attrs.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
            return null;

        // if default parameter constraint is not presented
        if (attrs.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint) && !target.HasDefaultConstructor())
            return null;

        // ensure all parameter constraints are implemented
        var meetsConstraints = type.GetGenericParameterConstraints()
            .All(constraint => constraint.ResolveGenericArgumentsByImplementation(target) != null);

        return meetsConstraints ? new[] { type } : null;
    }

    private static Type[]? ResolveGenericParameterArgumentsByStruct(this Type type, Type target)
    {
        var attrs = type.GenericParameterAttributes;

        // if reference type constraint is not presented
        if (attrs.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
            return null;

        // if not nullable value type constraint is not presented
        if (attrs.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint) && !target.IsNotNullableValueType())
            return null;

        // if default parameter constraint is not presented
        if (attrs.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint) && !target.HasDefaultConstructor())
            return null;

        // ensure all parameter constraints are implemented
        var meetsConstraints = type.GetGenericParameterConstraints()
            .All(constraint => target.ResolveGenericArgumentsByImplementation(constraint) != null);

        return meetsConstraints ? new[] { type } : null;
    }

    private static Type[]? ResolveGenericParameterArgumentsByInterface(this Type type, Type target)
    {
        var attrs = type.GenericParameterAttributes;

        // if reference type constraint is not presented
        if (attrs.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
            return null;

        // if not nullable value type constraint is not presented
        if (attrs.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
            return null;

        // if default parameter constraint is not presented
        if (attrs.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
            return null;

        // ensure all parameter constraints are implemented
        var meetsConstraints = type.GetGenericParameterConstraints()
            .All(constraint => target.ResolveGenericArgumentsByImplementation(constraint) != null);

        return meetsConstraints ? new[] { type } : null;
    }

    private static Type[]? ResolveClassArgumentsByGenericParameter(this Type type, Type target)
    {
        var attrs = target.GenericParameterAttributes;

        // if not nullable value type constraint is not presented
        if (attrs.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
            return null;

        // if default parameter constraint is not presented
        if (attrs.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint) && !type.HasDefaultConstructor())
            return null;

        // ensure all parameter constraints are implemented
        var meetsConstraints = target.GetGenericParameterConstraints()
            .All(constraint => type.ResolveGenericArgumentsByImplementation(constraint) != null);

        return meetsConstraints ? type.GetGenericArguments() : null;
    }

    private static Type[]? ResolveClassArgumentsByClass(this Type type, Type target)
    {
        if (type.GetGenericTypeDefinition() == target.GetGenericTypeDefinition())
            return BuildArgs(type, type, target);

        var baseType = type.BaseType;

        // if no base type or it's not generic - resolution fails, cause types' generic definitions are different
        if (baseType is null || !baseType.IsGenericType)
            return null;

        // base type is generic class type with same base definition, as target
        if (baseType.GetGenericTypeDefinition() == target.GetGenericTypeDefinition())
            return BuildArgs(type, baseType, target);

        // try resolve base type
        return ResolveBase(type, target);
    }

    private static Type[]? ResolveClassArgumentsByInterface(this Type type, Type target)
    {
        // find interface, that is implementation of target's generic definition
        var targetBase = target.GetGenericTypeDefinition();
        var implementation = type.GetOwnInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == targetBase);

        // implementation is generic interface type with same base definition, as target
        if (implementation != null)
            return BuildArgs(type, implementation, target);

        if (type.BaseType is null)
            return null;

        // try resolve base type
        return ResolveBase(type, target);
    }

    private static Type[]? ResolveStructArgumentsByGenericParameter(this Type type, Type target)
    {
        var attrs = target.GenericParameterAttributes;

        // if reference type constraint is not presented
        if (attrs.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
            return null;

        // if not nullable value type constraint is not presented
        if (attrs.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint) && !type.IsNotNullableValueType())
            return null;

        // if default parameter constraint is not presented
        if (attrs.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint) && !type.HasDefaultConstructor())
            return null;

        // ensure all parameter constraints are implemented
        var meetsConstraints = target.GetGenericParameterConstraints()
            .All(constraint => type.ResolveGenericArgumentsByImplementation(constraint) != null);

        return meetsConstraints ? type.GetGenericArguments() : null;
    }

    private static Type[]? ResolveStructArgumentsByStruct(this Type type, Type target)
    {
        if (type.GetGenericTypeDefinition() == target.GetGenericTypeDefinition())
            return BuildArgs(type, type, target);

        return null;
    }

    private static Type[]? ResolveStructArgumentsByInterface(this Type type, Type target)
    {
        // find interface, that is implementation of target's generic definition
        var targetBase = target.GetGenericTypeDefinition();
        var implementation = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == targetBase);

        if (implementation is null)
            return null;

        // implementation is generic interface type with same base definition, as target
        return BuildArgs(type, implementation, target);
    }

    private static Type[]? ResolveInterfaceArgumentsByGenericParameter(this Type type, Type target)
    {
        var attrs = target.GenericParameterAttributes;

        // if reference type constraint
        if (attrs.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
            return null;

        // if not nullable value type constraint
        if (attrs.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
            return null;

        // if default parameter constraint
        if (attrs.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
            return null;

        // ensure all parameter constraints are implemented
        var meetsConstraints = target.GetGenericParameterConstraints()
            .All(constraint => type.ResolveGenericArgumentsByImplementation(constraint) != null);

        return meetsConstraints ? type.GetGenericArguments() : null;
    }

    private static Type[]? ResolveInterfaceArgumentsByInterface(this Type type, Type target)
    {
        if (type.GetGenericTypeDefinition() == target.GetGenericTypeDefinition())
            return BuildArgs(type, type, target);

        // find interface, that is implementation of target's generic definition
        var targetBase = target.GetGenericTypeDefinition();
        var implementation = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == targetBase);

        if (implementation is null)
            return null;

        // implementation is generic interface type with same base definition, as target
        return BuildArgs(type, implementation, target);
    }

    private static Type[]? ResolveBase(Type type, Type target)
    {
        var unboundBaseType = type.GetUnboundBaseType();
        var baseArgs = unboundBaseType!.ResolveGenericArgumentsByImplementation(target);
        if (baseArgs is null)
            return null;

        if (!type.BaseType!.GetGenericTypeDefinition().TryMakeGenericType(out var baseImplementation, baseArgs))
            return null;

        return type.ResolveGenericArgumentsByImplementation(baseImplementation!);
    }

    private static Type[]? BuildArgs(Type type, Type source, Type target)
    {
        var args = type.GetGenericArguments();

        var succeed = FillArgs(args, source, target);
        if (!succeed)
            return null;

        var unresolvedArgs = args.Count(a => a.IsGenericTypeParameter);
        if (unresolvedArgs == 0 || unresolvedArgs == args.Length)
            return args;

        var originalArgs = type.GetGenericArguments();

        while (true)
        {
            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (arg.IsGenericTypeParameter)
                    continue;

                foreach (var constraint in originalArgs[i].GetGenericParameterConstraints())
                    if (!FillArgs(args, constraint, arg))
                        return null;
            }

            var currentlyUnresolved = args.Count(a => a.IsGenericTypeParameter);
            if (currentlyUnresolved == 0 || currentlyUnresolved == unresolvedArgs)
                break;

            unresolvedArgs = currentlyUnresolved;
        }

        return args;
    }

    private static bool FillArgs(Type[] args, Type source, Type target)
    {
        var implementation = target.GetTargetImplementation(source);
        if (implementation is null)
            return false;

        target = implementation;
        Type[] sourceArgs;
        Type[] targetArgs;
        if (target.IsArray)
        {
            sourceArgs = new[] { source.GetElementType() ! };
            targetArgs = new[] { target.GetElementType() ! };
        }
        else if (target.IsGenericType)
        {
            sourceArgs = source.GetGenericArguments();
            targetArgs = target.GetGenericArguments();
        }
        else
            return false;

        for (var i = 0; i < sourceArgs.Length; i++)
        {
            if (sourceArgs[i].IsGenericParameter)
                args[sourceArgs[i].GenericParameterPosition] = targetArgs[i];
            else if (sourceArgs[i].ContainsGenericParameters)
            {
                if (!FillArgs(args, sourceArgs[i], targetArgs[i]))
                    return false;
            }
        }

        return true;
    }

    private static NotImplementedException GetException(Type type, Type target) =>
        new($"Can't resolve {type.Name} generic arguments by implementation {target.Name}");
}