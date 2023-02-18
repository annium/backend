using System;
using System.Linq;

// ReSharper disable CheckNamespace
namespace Annium.Core.Reflection;

public static class IsDerivedFromExtension
{
    public static bool IsDerivedFrom(this Type type, Type target)
    {
        // if target is not generic type definition - simply check with IsAssignable from
        if (!target.IsGenericTypeDefinition)
            return target.IsAssignableFrom(type);

        if (target.IsClass)
            return type.GetInheritanceChain(true, true).Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == target);

        if (target.IsInterface)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == target)
                return true;

            return type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == target);
        }

        throw new InvalidOperationException($"Type '{target}' cannot be derived from");
    }
}