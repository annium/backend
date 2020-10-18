using System.Reflection;
using Annium.Core.Runtime.Internal.Resources;
using Annium.Core.Runtime.Internal.Types;
using Annium.Core.Runtime.Resources;
using Annium.Core.Runtime.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRuntimeTools(
            this IServiceCollection services,
            Assembly assembly,
            bool tryLoadReferences
        )
        {
            services.AddSingleton(TypeManager.GetInstance(assembly, tryLoadReferences));
            services.TryAddSingleton<ITypeResolver, TypeResolver>();

            return services;
        }

        public static IServiceCollection AddResourceLoader(this IServiceCollection services)
        {
            services.AddSingleton<IResourceLoader, ResourceLoader>();

            return services;
        }
    }
}