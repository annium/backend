using System;
using System.Linq;

namespace Annium.Core.Reflection
{
    internal static class ResolveGenericArgumentsByImplentationsExtension
    {
        public static Type[]? ResolveGenericArgumentsByImplentations(this Type type, params Type[] targets)
        {
            var argsMatrix = targets
                .Select(t =>
                    type.ResolveGenericArgumentsByImplentation(t)?
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
    }
}