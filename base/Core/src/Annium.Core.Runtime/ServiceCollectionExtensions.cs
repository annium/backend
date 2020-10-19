using System;
using System.Linq;
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

        public static ITypeManager GetTypeManager(this IServiceCollection services)
        {
            var descriptors = services.Where(x => x.ServiceType == typeof(ITypeManager)).ToArray();

            if (descriptors.Length != 1)
                throw new InvalidOperationException($"Single {nameof(ITypeManager)} instance must be registered.");

            var descriptor = descriptors[0];
            if (descriptor.ImplementationInstance is null)
                throw new InvalidOperationException($"{nameof(ITypeManager)} must be registered with instance.");

            return (ITypeManager) descriptor.ImplementationInstance;
        }
    }
}