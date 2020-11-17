using System;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static partial class ServiceContainerExtensions
    {
        #region Add

        public static IServiceContainer AddTransient<TService>(this IServiceContainer container)
            where TService : class
        {
            return container.AddItem(ServiceDescriptor.Transient<TService, TService>());
        }

        public static IServiceContainer AddTransient<TService, TImplementation>(this IServiceContainer container)
            where TService : class
            where TImplementation : class, TService
        {
            return container.AddItem(ServiceDescriptor.Transient<TService, TImplementation>());
        }

        public static IServiceContainer AddTransient<TService>(
            this IServiceContainer container,
            Func<IServiceProvider, TService> implementationFactory
        )
            where TService : class
        {
            return container.AddItem(ServiceDescriptor.Transient(implementationFactory));
        }

        public static IServiceContainer AddTransient<TService, TImplementation>(
            this IServiceContainer container,
            Func<IServiceProvider, TImplementation> implementationFactory
        )
            where TService : class
            where TImplementation : class, TService
        {
            return container.AddItem(ServiceDescriptor.Transient<TService, TImplementation>(implementationFactory));
        }

        public static IServiceContainer AddTransient(
            this IServiceContainer container,
            Type serviceType
        )
        {
            return container.AddItem(ServiceDescriptor.Transient(serviceType, serviceType));
        }

        public static IServiceContainer AddTransient(
            this IServiceContainer container,
            Type serviceType,
            Func<IServiceProvider, object> implementationFactory
        )
        {
            return container.AddItem(ServiceDescriptor.Transient(serviceType, implementationFactory));
        }

        public static IServiceContainer AddTransient(
            this IServiceContainer container,
            Type serviceType,
            Type implementationType
        )
        {
            return container.AddItem(ServiceDescriptor.Transient(serviceType, implementationType));
        }

        #endregion

        #region TryAdd

        public static IServiceContainer TryAddTransient<TService>(this IServiceContainer container)
            where TService : class
        {
            return container.TryAddItem(ServiceDescriptor.Transient<TService, TService>());
        }

        public static IServiceContainer TryAddTransient<TService, TImplementation>(this IServiceContainer container)
            where TService : class
            where TImplementation : class, TService
        {
            return container.TryAddItem(ServiceDescriptor.Transient<TService, TImplementation>());
        }

        public static IServiceContainer TryAddTransient<TService>(
            this IServiceContainer container,
            Func<IServiceProvider, TService> implementationFactory
        )
            where TService : class
        {
            return container.TryAddItem(ServiceDescriptor.Transient(implementationFactory));
        }

        public static IServiceContainer TryAddTransient<TService, TImplementation>(
            this IServiceContainer container,
            Func<IServiceProvider, TImplementation> implementationFactory
        )
            where TService : class
            where TImplementation : class, TService
        {
            return container.TryAddItem(ServiceDescriptor.Transient<TService, TImplementation>(implementationFactory));
        }

        public static IServiceContainer TryAddTransient(
            this IServiceContainer container,
            Type serviceType
        )
        {
            return container.TryAddItem(ServiceDescriptor.Transient(serviceType, serviceType));
        }

        public static IServiceContainer TryAddTransient(
            this IServiceContainer container,
            Type serviceType,
            Func<IServiceProvider, object> implementationFactory
        )
        {
            return container.TryAddItem(ServiceDescriptor.Transient(serviceType, implementationFactory));
        }

        public static IServiceContainer TryAddTransient(
            this IServiceContainer container,
            Type serviceType,
            Type implementationType
        )
        {
            return container.TryAddItem(ServiceDescriptor.Transient(serviceType, implementationType));
        }

        #endregion
    }
}