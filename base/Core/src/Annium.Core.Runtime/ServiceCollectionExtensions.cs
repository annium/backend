using Annium.Core.Runtime.Loader;
using Annium.Core.Runtime.Resources;
using Annium.Core.Runtime.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddRuntimeTools(this IServiceCollection services)
        {
            services.AddSingleton<ITypeManager>(TypeManager.Instance);
        }

        public static IServiceCollection AddLoadContextFactories(this IServiceCollection services)
        {
            services.AddSingleton<PluginLoadContextFactory>();

            return services;
        }

        public static IServiceCollection AddResourceLoader(this IServiceCollection services)
        {
            services.AddSingleton<IResourceLoader, ResourceLoader>();

            return services;
        }
    }
}