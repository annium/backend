using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection.Obsolete.Internal
{
    internal static class ServiceCollectionExtensions
    {
        [Obsolete]
        public static IServiceCollection AddChecked(this IServiceCollection services, ServiceDescriptor descriptor)
        {
            // skip if descriptor has ImplementationType and it is already registered
            if (
                descriptor.ImplementationType != null &&
                services.Any(x =>
                    x.ServiceType == descriptor.ServiceType &&
                    x.ImplementationType == descriptor.ImplementationType
                )
            )
                return services;

            // skip if descriptor has ImplementationInstance and it is already registered
            if (
                descriptor.ImplementationInstance != null &&
                services.Any(x =>
                    x.ServiceType == descriptor.ServiceType &&
                    x.ImplementationInstance?.Equals(descriptor.ImplementationInstance) == true
                )
            )
                return services;

            services.Add(descriptor);

            return services;
        }
    }
}