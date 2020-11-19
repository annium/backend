using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceContainerExtensions
    {
        // public static IBulkRegistrationBuilderBase AddAll(this IServiceContainer container)
        //     => container.Add(container.GetTypeManager().Types.AsEnumerable());
        //
        // public static IBulkRegistrationBuilderBase AddAll(this IServiceContainer container, Assembly assembly, bool tryLoadReferences)
        //     => container.Add(TypeManager.GetInstance(assembly, tryLoadReferences).Types.AsEnumerable());

        public static IBulkRegistrationBuilderBase AddAll(this IServiceContainer container, Assembly assembly)
            => container.Add(assembly.GetTypes().AsEnumerable());

        public static IServiceContainer Clone(this IServiceContainer container)
        {
            var clone = new ServiceContainer();

            foreach (var descriptor in container)
                clone.Add(descriptor);

            return clone;
        }

        public static ServiceProvider BuildServiceProvider(this IServiceContainer container) =>
            container.Collection.BuildServiceProvider();
    }
}