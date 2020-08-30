using System.Reflection;
using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.Mapper
{
    public static class Mapper
    {
        public static IMapper GetFor(Assembly assembly)
        {
            var services = new ServiceCollection();
            services.AddRuntimeTools(assembly, false);
            services.AddMapper(false);

            var provider = services.BuildServiceProvider();

            return provider.GetRequiredService<IMapper>();
        }
    }
}