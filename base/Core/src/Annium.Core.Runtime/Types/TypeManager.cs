using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Annium.Core.Runtime.Internal.Types;

namespace Annium.Core.Runtime.Types;

public static class TypeManager
{
    private static readonly ConcurrentDictionary<CacheKey, ITypeManager> Instances =
        new();

    public static ITypeManager GetInstance(
        Assembly assembly,
        bool tryLoadReferences
    )
    {
        var key = new CacheKey(assembly, tryLoadReferences);
        return Instances.GetOrAdd(key, key => new TypeManagerInstance(key.Assembly, key.TryLoadReferences));
    }

    public static void Release(Assembly assembly)
    {
        foreach (var key in Instances.Keys.Where(x => x.Assembly == assembly).ToArray())
            Instances.TryRemove(key, out _);
    }

    private record CacheKey(Assembly Assembly, bool TryLoadReferences)
    {
        public virtual bool Equals(CacheKey? other) => GetHashCode() == other?.GetHashCode();

        public override int GetHashCode() => HashCode.Combine(Assembly, TryLoadReferences);
    }
}