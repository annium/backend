using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.Internal;
using Annium.Core.Reflection;
using Annium.Core.Runtime.Types;

namespace Annium.Core.Runtime.Internal.Types;

internal class TypeResolver : ITypeResolver
{
    private readonly ITypeManager _typeManager;

    public TypeResolver(ITypeManager typeManager)
    {
        _typeManager = typeManager;
    }

    public IReadOnlyCollection<Type> ResolveType(Type type)
    {
        if (!type.IsGenericType)
            return new[] { type };

        this.Trace($"{type.FriendlyName()} - start");
        var sets = new List<Type[]>();
        foreach (var argument in type.GetGenericArguments())
        {
            if (argument.GetGenericParameterConstraints().Length == 0)
                throw new ArgumentException($"Can't use generic Profile {type} with unconstrained parameter {argument}");

            var implementations = _typeManager.Types
                .Where(x => !x.ContainsGenericParameters)
                .Select(x => x.GetTargetImplementation(argument))
                .Where(x => x != null)
                .ToArray();
            sets.Add(implementations!);
        }

        var combinations = GetCombinations(sets).ToArray();
        var types = combinations
            .Select(type.MakeGenericType)
            .ToArray();

        this.Trace($"{type.FriendlyName()} - end");

        return types;
    }

    private IReadOnlyCollection<Type[]> GetCombinations(IReadOnlyCollection<Type[]> sets)
    {
        if (sets.Count == 1)
            return sets.ElementAt(0).Select(x => new[] { x }).ToArray();

        var result = sets.ElementAt(0)
            .SelectMany(x => GetCombinations(sets.Skip(1).ToArray()).Select(y => y.Prepend(x)))
            .Select(x => x.ToArray())
            .ToArray();

        return result;
    }
}