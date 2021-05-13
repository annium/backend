using System;
using System.Collections.Concurrent;
using System.Reflection;
using Annium.Core.DependencyInjection;
using Annium.Core.Primitives;

namespace Annium.Core.Mapper
{
    public static class Mapper
    {
        private static readonly ConcurrentDictionary<CacheKey, IMapper> Mappers = new();

        public static IMapper GetFor(Assembly assembly, params string[] patterns) =>
            Mappers.GetOrAdd(new CacheKey(assembly, patterns), key =>
            {
                var container = new ServiceContainer();
                container.AddRuntimeTools(key.Assembly, false, key.Patterns);
                container.AddMapper(false);

                var provider = container.BuildServiceProvider();

                return provider.Resolve<IMapper>();
            });

        private record CacheKey
        {
            public Assembly Assembly { get; }
            public string[] Patterns { get; }

            public CacheKey(
                Assembly assembly,
                string[] patterns
            )
            {
                Assembly = assembly;
                Patterns = patterns;
            }

            public virtual bool Equals(CacheKey? other) => GetHashCode() == other?.GetHashCode();

            public override int GetHashCode() => HashCode.Combine(
                Assembly,
                HashCodeSeq.Combine(Patterns)
            );
        }
    }
}