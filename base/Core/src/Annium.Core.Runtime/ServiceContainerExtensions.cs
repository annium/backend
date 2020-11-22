using System;
using System.Linq;
using System.Reflection;
using Annium.Core.Runtime.Internal.Resources;
using Annium.Core.Runtime.Internal.Types;
using Annium.Core.Runtime.Resources;
using Annium.Core.Runtime.Types;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceContainerExtensions
    {
        public static IServiceContainer AddRuntimeTools(
            this IServiceContainer container,
            Assembly assembly,
            bool tryLoadReferences
        )
        {
            container.Add(TypeManager.GetInstance(assembly, tryLoadReferences)).As<ITypeManager>().Singleton();
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

        public static IBulkRegistrationBuilderBase AddAll(this IServiceContainer container)
            => container.Add(container.GetTypeManager().Types.AsEnumerable());

        public static IBulkRegistrationBuilderBase AddAll(this IServiceContainer container, Assembly assembly, bool tryLoadReferences)
            => container.Add(TypeManager.GetInstance(assembly, tryLoadReferences).Types.AsEnumerable());

        public static IBulkRegistrationBuilderBase AddAll(this IServiceContainer container, Assembly assembly)
            => container.Add(assembly.GetTypes().AsEnumerable());
    }
}