using System;
using System.Linq;
using System.Reflection;
using Annium.Core.DependencyInjection.Obsolete;
using Annium.Core.DependencyInjection.Obsolete.Internal;
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
        public static IServiceContainer AddRuntimeTools(
            this IServiceContainer container,
            Assembly assembly,
            bool tryLoadReferences
        )
        {
            container.Add(TypeManager.GetInstance(assembly, tryLoadReferences)).As<ITypeManager>();
            container.Add<TypeResolver>().As<ITypeResolver>().Singleton();

            return container;
        }

        public static IServiceContainer AddResourceLoader(this IServiceContainer container)
        {
            container.Add<ResourceLoader>().As<IResourceLoader>().Singleton();

            return container;
        }

        public static ITypeManager GetTypeManager(this IServiceContainer container)
        {
            var descriptors = container.Where(x => x.ServiceType == typeof(ITypeManager)).ToArray();

            if (descriptors.Length != 1)
                throw new InvalidOperationException($"Single {nameof(ITypeManager)} instance must be registered.");

            var descriptor = descriptors[0];
            if (descriptor is IInstanceServiceDescriptor instanceDescriptor)
                return (ITypeManager) instanceDescriptor.ImplementationInstance;

            throw new InvalidOperationException($"{nameof(ITypeManager)} must be registered with instance.");
        }

        [Obsolete]
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

        [Obsolete]
        public static IServiceCollection AddResourceLoader(this IServiceCollection services)
        {
            services.AddSingleton<IResourceLoader, ResourceLoader>();

            return services;
        }

        [Obsolete]
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

        [Obsolete]
        public static IRegistrationBuilder AddAllTypes(this IServiceCollection services)
        {
            var typeManager = services.GetTypeManager();

            return new RegistrationBuilder(services, typeManager.Types);
        }

        [Obsolete]
        public static IRegistrationBuilder AddAllTypes(this IServiceCollection services, Assembly assembly, bool tryLoadReferences)
            => new RegistrationBuilder(services, TypeManager.GetInstance(assembly, tryLoadReferences).Types);
    }
}