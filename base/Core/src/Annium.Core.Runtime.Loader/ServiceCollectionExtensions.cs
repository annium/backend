using Annium.Core.Runtime.Loader;
using Annium.Core.Runtime.Loader.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAssemblyLoader(this IServiceCollection services)
        {
            services.AddTransient<IAssemblyLoaderBuilder, AssemblyLoaderBuilder>();

            return services;
        }
    }
}