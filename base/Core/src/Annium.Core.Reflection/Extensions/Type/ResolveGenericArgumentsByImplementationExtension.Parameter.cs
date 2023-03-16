using System;
using System.Linq;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Annium.Core.Reflection;

public static partial class ResolveGenericArgumentsByImplementationExtension
{
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
        var typeConstraints = type.GetGenericParameterConstraints();
        var targetConstraints = target.GetGenericParameterConstraints();
        foreach (var targetConstraint in targetConstraints)
        {
            var meetsConstraint = false;
            foreach (var typeConstraint in typeConstraints)
            {
                var constraintArgs = typeConstraint.ResolveGenericArgumentsByImplementation(targetConstraint);
                if (constraintArgs is null)
                    continue;

                meetsConstraint = true;
                break;
            }

            if (!meetsConstraint)
                return null;
        }

        return new[] { type };
    }

    private static Type[]? ResolveGenericParameterArgumentsByClass(this Type type, Type target)
    {
        var typeAttrs = type.GenericParameterAttributes;

        // if not nullable value type constraint is not presented
        if (typeAttrs.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
            return null;

        // if default parameter constraint is not presented
        if (typeAttrs.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint) && !target.HasDefaultConstructor())
            return null;

        // ensure all parameter constraints are implemented
        var meetsConstraints = type.GetGenericParameterConstraints()
            .All(constraint => constraint.ResolveGenericArgumentsByImplementation(target) != null);

        return meetsConstraints ? new[] { target } : null;
    }

    private static Type[]? ResolveGenericParameterArgumentsByStruct(this Type type, Type target)
    {
        var typeAttrs = type.GenericParameterAttributes;

        // if reference type constraint is not presented
        if (typeAttrs.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
            return null;

        // if not nullable value type constraint is not presented
        if (typeAttrs.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint) && !target.IsNotNullableValueType())
            return null;

        // if default parameter constraint is not presented
        if (typeAttrs.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint) && !target.HasDefaultConstructor())
            return null;

        // ensure all parameter constraints are implemented
        var meetsConstraints = type.GetGenericParameterConstraints()
            .All(constraint => target.ResolveGenericArgumentsByImplementation(constraint) != null);

        return meetsConstraints ? new[] { target } : null;
    }

    private static Type[]? ResolveGenericParameterArgumentsByInterface(this Type type, Type target)
    {
        var typeAttrs = type.GenericParameterAttributes;

        // if reference type constraint is not presented
        if (typeAttrs.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
            return null;

        // if not nullable value type constraint is not presented
        if (typeAttrs.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
            return null;

        // if default parameter constraint is not presented
        if (typeAttrs.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
            return null;

        // ensure all parameter constraints are implemented
        var meetsConstraints = type.GetGenericParameterConstraints()
            .All(constraint => target.ResolveGenericArgumentsByImplementation(constraint) != null);

        return meetsConstraints ? new[] { target } : null;
    }
}