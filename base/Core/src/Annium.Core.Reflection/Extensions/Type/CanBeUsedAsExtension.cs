using System;

// ReSharper disable once CheckNamespace
namespace Annium.Core.Reflection;

public static class CanBeUsedAsExtension
{
    public static bool CanBeUsedAs(this Type type, Type parameter)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        if (parameter is null)
            throw new ArgumentNullException(nameof(parameter));

        if (type.ContainsGenericParameters)
            throw new InvalidOperationException($"Type {parameter.FriendlyName()} is not expected to contain generic parameters");

        if (!parameter.IsGenericParameter)
            throw new InvalidOperationException($"Type {type.FriendlyName()} is not generic parameter");

        var constraints = parameter.GetGenericParameterConstraints();

        foreach (var constraint in constraints)
        {

            var constraintArgs = type.ResolveGenericArgumentsByImplementation(constraint);
            if (constraintArgs is null)
                return false;
        }

        return true;
    }
}