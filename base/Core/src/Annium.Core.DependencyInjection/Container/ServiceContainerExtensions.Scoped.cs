using System;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static partial class ServiceContainerExtensions
    {
        #region Add

        public static IServiceContainer AddScoped<TService>(this IServiceContainer container)
            where TService : class
        {
            return container.AddItem(ServiceDescriptor.Scoped<TService, TService>());
        }

        public static IServiceContainer AddScoped<TService, TImplementation>(this IServiceContainer container)
            where TService : class
            where TImplementation : class, TService
        {
            return container.AddItem(ServiceDescriptor.Scoped<TService, TImplementation>());
        }

        public static IServiceContainer AddScoped<TService>(
            this IServiceContainer container,
            Func<IServiceProvider, TService> implementationFactory
        )
            where TService : class
        {
            return container.AddItem(ServiceDescriptor.Scoped(implementationFactory));
        }

        public static IServiceContainer AddScoped<TService, TImplementation>(
            this IServiceContainer container,
            Func<IServiceProvider, TImplementation> implementationFactory
        )
            where TService : class
            where TImplementation : class, TService
        {
            return container.AddItem(ServiceDescriptor.Scoped<TService, TImplementation>(implementationFactory));
        }

        public static IServiceContainer AddScoped(
            this IServiceContainer container,
            Type serviceType
        )
        {
            return container.AddItem(ServiceDescriptor.Scoped(serviceType, serviceType));
        }

        public static IServiceContainer AddScoped(
            this IServiceContainer container,
            Type serviceType,
            Func<IServiceProvider, object> implementationFactory
        )
        {
            return container.AddItem(ServiceDescriptor.Scoped(serviceType, implementationFactory));
        }

        public static IServiceContainer AddScoped(
            this IServiceContainer container,
            Type serviceType,
            Type implementationType
        )
        {
            return container.AddItem(ServiceDescriptor.Scoped(serviceType, implementationType));
        }

        #endregion

        #region TryAdd

        public static IServiceContainer TryAddScoped<TService>(this IServiceContainer container)
            where TService : class
        {
            return container.TryAddItem(ServiceDescriptor.Scoped<TService, TService>());
        }

        public static IServiceContainer TryAddScoped<TService, TImplementation>(this IServiceContainer container)
            where TService : class
            where TImplementation : class, TService
        {
            return container.TryAddItem(ServiceDescriptor.Scoped<TService, TImplementation>());
        }

        public static IServiceContainer TryAddScoped<TService>(
            this IServiceContainer container,
            Func<IServiceProvider, TService> implementationFactory
        )
            where TService : class
        {
            return container.TryAddItem(ServiceDescriptor.Scoped(implementationFactory));
        }

        public static IServiceContainer TryAddScoped<TService, TImplementation>(
            this IServiceContainer container,
            Func<IServiceProvider, TImplementation> implementationFactory
        )
            where TService : class
            where TImplementation : class, TService
        {
            return container.TryAddItem(ServiceDescriptor.Scoped<TService, TImplementation>(implementationFactory));
        }

        public static IServiceContainer TryAddScoped(
            this IServiceContainer container,
            Type serviceType
        )
        {
            return container.TryAddItem(ServiceDescriptor.Scoped(serviceType, serviceType));
        }

        public static IServiceContainer TryAddScoped(
            this IServiceContainer container,
            Type serviceType,
            Func<IServiceProvider, object> implementationFactory
        )
        {
            return container.TryAddItem(ServiceDescriptor.Scoped(serviceType, implementationFactory));
        }

        public static IServiceContainer TryAddScoped(
            this IServiceContainer container,
            Type serviceType,
            Type implementationType
        )
        {
            return container.TryAddItem(ServiceDescriptor.Scoped(serviceType, implementationType));
        }

        #endregion
    }
}