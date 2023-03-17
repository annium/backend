using System;
using System.Linq;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Annium.Core.Reflection;

public static partial class ResolveGenericArgumentsByImplementationExtension
{
    private static Type[]? ResolveStructArgumentsByGenericParameter(this Type type, Type target)
    {
        if (type.TryGetTargetImplementation(target, out var args))
            return args;

        // as of here:
        // - type is open generic type with generic parameters
        // - target is open/defined generic type with/without generic parameters

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
        if (type.TryGetTargetImplementation(target, out var args))
            return args;

        if (type.TryCheckAssignableFrom(target, out args))
            return args;

        // as of here:
        // - type is open generic type with generic parameters
        // - target is open/defined generic type with/without generic parameters

        if (type.GetGenericTypeDefinition() == target.GetGenericTypeDefinition())
            return BuildArgs(type, type, target);

        return null;
    }

    private static Type[]? ResolveStructArgumentsByInterface(this Type type, Type target)
    {
        if (type.TryGetTargetImplementation(target, out var args))
            return args;

        if (type.TryCheckAssignableFrom(target, out args))
            return args;

        // as of here:
        // - type is open generic type with generic parameters
        // - target is open/defined generic type with/without generic parameters

        // find interface, that is implementation of target's generic definition
        var targetBase = target.GetGenericTypeDefinition();
        var implementation = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == targetBase);

        if (implementation is null)
            return null;

        // implementation is generic interface type with same base definition, as target
        return BuildArgs(type, implementation, target);
    }
}