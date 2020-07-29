using System.Reflection;
using Annium.Core.Runtime.Resources;
using Annium.Core.Runtime.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddRuntimeTools(
            this IServiceCollection services,
            Assembly assembly
        )
        {
            services.AddSingleton(TypeManager.GetInstance(assembly));
        }

        public static IServiceCollection AddResourceLoader(this IServiceCollection services)
        {
            services.AddSingleton<IResourceLoader, ResourceLoader>();

            return services;
        }
    }
}