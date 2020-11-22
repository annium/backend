using System.Reflection;
using Annium.Core.DependencyInjection;

namespace Annium.Core.Mapper
{
    public static class Mapper
    {
        public static IMapper GetFor(Assembly assembly)
        {
            var container = new ServiceContainer();
            container.AddRuntimeTools(assembly, false);
            container.AddMapper(false);

            var provider = container.BuildServiceProvider();

            return provider.Resolve<IMapper>();
        }
    }
}