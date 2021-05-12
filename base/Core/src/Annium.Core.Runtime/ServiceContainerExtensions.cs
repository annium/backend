using System;
using System.Linq;
using System.Reflection;
using Annium.Core.Runtime.Internal.Resources;
using Annium.Core.Runtime.Internal.Time;
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
            bool tryLoadReferences,
            params string[] patterns
        )
        {
            container.Add(TypeManager.GetInstance(assembly, tryLoadReferences, patterns)).As<ITypeManager>().Singleton();
            container.Add<ITypeResolver, TypeResolver>().Singleton();

            return container;
        }

        public static IServiceContainer AddResourceLoader(this IServiceContainer container)
        {
            container.Add<IResourceLoader, ResourceLoader>().Singleton();

            return container;
        }

        public static IServiceContainer AddTimeProvider(this IServiceContainer container)
        {
            container.Add<TimeProvider>().AsInterfaces().Singleton();

            return container;
        }

        public static IServiceContainer AddTestTimeProvider(this IServiceContainer container)
        {
            container.Add<TestTimeProvider>().AsInterfaces().Singleton();

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

        public static IBulkRegistrationBuilderBase AddAll(this IServiceContainer container, Assembly assembly)
            => container.Add(assembly.GetTypes().AsEnumerable());
    }
}