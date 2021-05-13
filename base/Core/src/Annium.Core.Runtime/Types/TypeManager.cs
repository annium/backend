using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Annium.Core.Primitives;
using Annium.Core.Runtime.Internal.Types;

namespace Annium.Core.Runtime.Types
{
    public static class TypeManager
    {
        private static readonly ConcurrentDictionary<CacheKey, ITypeManager> Instances =
            new();

        public static ITypeManager GetInstance(
            Assembly assembly,
            bool tryLoadReferences,
            string[] patterns
        )
        {
            var key = new CacheKey(assembly, tryLoadReferences, patterns);
            return Instances.GetOrAdd(key, key => new TypeManagerInstance(key.Assembly, key.TryLoadReferences, key.Patterns));
        }

        public static void Release(Assembly assembly)
        {
            foreach (var key in Instances.Keys.Where(x => x.Assembly == assembly).ToArray())
                Instances.TryRemove(key, out _);
        }

        private record CacheKey
        {
            public Assembly Assembly { get; }
            public bool TryLoadReferences { get; }
            public IReadOnlyCollection<string> Patterns { get; }

            public CacheKey(
                Assembly assembly,
                bool tryLoadReferences,
                IReadOnlyCollection<string> patterns
            )
            {
                Assembly = assembly;
                TryLoadReferences = tryLoadReferences;
                Patterns = patterns;
            }

            public virtual bool Equals(CacheKey? other) => GetHashCode() == other?.GetHashCode();

            public override int GetHashCode() => HashCode.Combine(
                Assembly,
                TryLoadReferences,
                HashCodeSeq.Combine(Patterns)
            );
        }
    }
}