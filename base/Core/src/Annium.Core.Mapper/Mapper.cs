using System.Collections.Concurrent;
using System.Reflection;
using Annium.Core.DependencyInjection;

namespace Annium.Core.Mapper
{
    public static class Mapper
    {
        private static readonly ConcurrentDictionary<Assembly, IMapper> Mappers = new();

        public static IMapper GetFor(Assembly assembly) => Mappers.GetOrAdd(assembly, x =>
        {
            var container = new ServiceContainer();
            container.AddRuntimeTools(x, false);
            container.AddMapper(false);

            var provider = container.BuildServiceProvider();

            return provider.Resolve<IMapper>();
        });
    }
}