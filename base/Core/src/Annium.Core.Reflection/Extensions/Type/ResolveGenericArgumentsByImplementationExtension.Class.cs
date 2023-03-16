using System;
using System.Linq;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Annium.Core.Reflection;

public static partial class ResolveGenericArgumentsByImplementationExtension
{
    private static Type[]? ResolveClassArgumentsByGenericParameter(this Type type, Type target)
    {
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
        if (!type.IsGenericParameter && target is { IsGenericParameter: false, IsGenericType: false })
            return target.IsAssignableFrom(type) ? type.GetGenericArguments() : null;

        // as of here:
        // - type is open generic type with generic parameters
        // - target is open/defined generic type with/without generic parameters

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
        if (!type.IsGenericParameter && target is { IsGenericParameter: false, IsGenericType: false })
            return target.IsAssignableFrom(type) ? type.GetGenericArguments() : null;

        // as of here:
        // - type is open generic type with generic parameters
        // - target is open/defined generic type with/without generic parameters

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
        if (!type.IsGenericParameter && target is { IsGenericParameter: false, IsGenericType: false })
            return target.IsAssignableFrom(type) ? type.GetGenericArguments() : null;

        // as of here:
        // - type is open generic type with generic parameters
        // - target is open/defined generic type with/without generic parameters

        // find interface, that is implementation of target's generic definition
        var targetBase = target.GetGenericTypeDefinition();
        var implementation = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == targetBase);

        // implementation is generic interface type with same base definition, as target
        if (implementation != null)
            return BuildArgs(type, implementation, target);

        if (type.BaseType is null)
            return null;

        // try resolve base type
        return ResolveBase(type, target);
    }
}