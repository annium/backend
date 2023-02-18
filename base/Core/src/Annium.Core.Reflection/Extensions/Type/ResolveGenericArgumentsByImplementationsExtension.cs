using System;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Annium.Core.Reflection;

internal static class ResolveGenericArgumentsByImplementationsExtension
{
    public static Type[]? ResolveGenericArgumentsByImplementations(this Type type, params Type[] targets)
    {
        var argsMatrix = targets
            .Select(t =>
                type.ResolveGenericArgumentsByImplementation(t)?
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

                if (arg == otherArg)
                    args[i] = otherArg;
                else
                    return null;
            }

        return args;
    }
}