using System;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Annium.Core.Reflection;

public static partial class ResolveGenericArgumentsByImplementationExtension
{
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
            sourceArgs = new[] { source.GetElementType()! };
            targetArgs = new[] { target.GetElementType()! };
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
}