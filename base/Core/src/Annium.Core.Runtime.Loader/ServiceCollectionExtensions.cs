using Annium.Core.Runtime.Loader;
using Annium.Core.Runtime.Loader.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLoadContextFactories(this IServiceCollection services)
        {
            services.AddSingleton<IPluginLoadContextFactory, PluginLoadContextFactory>();

            return services;
        }
    }
}