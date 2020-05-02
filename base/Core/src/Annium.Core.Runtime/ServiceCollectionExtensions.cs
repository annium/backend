using Annium.Core.Runtime.Loader;
using Annium.Core.Runtime.Resources;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLoadContextFactories(this IServiceCollection services)
        {
            services.AddSingleton<DirectoryLoadContextFactory>();

            return services;
        }

        public static IServiceCollection AddResourceLoader(this IServiceCollection services)
        {
            services.AddSingleton<IResourceLoader, ResourceLoader>();

            return services;
        }
    }
}