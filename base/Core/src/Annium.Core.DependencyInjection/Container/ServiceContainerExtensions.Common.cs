using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static partial class ServiceContainerExtensions
    {
        public static IServiceContainer AddItem(this IServiceContainer container, ServiceDescriptor item)
        {
            container.Add(item);

            return container;
        }

        public static IServiceContainer TryAddItem(this IServiceContainer container, ServiceDescriptor item)
        {
            // skip if descriptor has Imp lementationType and it is already registered
            if (
                item.ImplementationType != null &&
                container.Any(x =>
                    x.ServiceType == item.ServiceType &&
                    x.ImplementationType == item.ImplementationType
                )
            )
                return container;

            // skip if descriptor has ImplementationInstance and it is already registered
            if (
                item.ImplementationInstance != null &&
                container.Any(x =>
                    x.ServiceType == item.ServiceType &&
                    x.ImplementationInstance?.Equals(item.ImplementationInstance) == true
                )
            )
                return container;

            container.Add(item);

            return container;
        }

        public static ServiceProvider BuildServiceProvider(this IServiceContainer container) =>
            container.Collection.BuildServiceProvider();
    }
}