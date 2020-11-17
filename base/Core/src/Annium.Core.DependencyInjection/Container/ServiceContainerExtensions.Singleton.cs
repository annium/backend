using System;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static partial class ServiceContainerExtensions
    {
        #region Add

        public static IServiceContainer AddSingleton<TService>(this IServiceContainer container)
            where TService : class
        {
            return container.AddItem(ServiceDescriptor.Singleton<TService, TService>());
        }

        public static IServiceContainer AddSingleton<TService, TImplementation>(this IServiceContainer container)
            where TService : class
            where TImplementation : class, TService
        {
            return container.AddItem(ServiceDescriptor.Singleton<TService, TImplementation>());
        }

        public static IServiceContainer AddSingleton<TService>(
            this IServiceContainer container,
            Func<IServiceProvider, TService> implementationFactory
        )
            where TService : class
        {
            return container.AddItem(ServiceDescriptor.Singleton(implementationFactory));
        }

        public static IServiceContainer AddSingleton<TService, TImplementation>(
            this IServiceContainer container,
            Func<IServiceProvider, TImplementation> implementationFactory
        )
            where TService : class
            where TImplementation : class, TService
        {
            return container.AddItem(ServiceDescriptor.Singleton<TService, TImplementation>(implementationFactory));
        }

        public static IServiceContainer AddSingleton<TService>(
            this IServiceContainer container,
            TService implementationInstance
        )
            where TService : class
        {
            return container.AddItem(ServiceDescriptor.Singleton(implementationInstance));
        }

        public static IServiceContainer AddSingleton(
            this IServiceContainer container,
            Type serviceType
        )
        {
            return container.AddItem(ServiceDescriptor.Singleton(serviceType, serviceType));
        }

        public static IServiceContainer AddSingleton(
            this IServiceContainer container,
            Type serviceType,
            Func<IServiceProvider, object> implementationFactory
        )
        {
            return container.AddItem(ServiceDescriptor.Singleton(serviceType, implementationFactory));
        }

        public static IServiceContainer AddSingleton(
            this IServiceContainer container,
            Type serviceType,
            Type implementationType
        )
        {
            return container.AddItem(ServiceDescriptor.Singleton(serviceType, implementationType));
        }

        public static IServiceContainer AddSingleton(
            this IServiceContainer container,
            Type serviceType,
            object implementationInstance
        )
        {
            return container.AddItem(ServiceDescriptor.Singleton(serviceType, implementationInstance));
        }

        #endregion

        #region TryAdd

        public static IServiceContainer TryAddSingleton<TService>(this IServiceContainer container)
            where TService : class
        {
            return container.TryAddItem(ServiceDescriptor.Singleton<TService, TService>());
        }

        public static IServiceContainer TryAddSingleton<TService, TImplementation>(this IServiceContainer container)
            where TService : class
            where TImplementation : class, TService
        {
            return container.TryAddItem(ServiceDescriptor.Singleton<TService, TImplementation>());
        }

        public static IServiceContainer TryAddSingleton<TService>(
            this IServiceContainer container,
            Func<IServiceProvider, TService> implementationFactory
        )
            where TService : class
        {
            return container.TryAddItem(ServiceDescriptor.Singleton(implementationFactory));
        }

        public static IServiceContainer TryAddSingleton<TService, TImplementation>(
            this IServiceContainer container,
            Func<IServiceProvider, TImplementation> implementationFactory
        )
            where TService : class
            where TImplementation : class, TService
        {
            return container.TryAddItem(ServiceDescriptor.Singleton<TService, TImplementation>(implementationFactory));
        }

        public static IServiceContainer TryAddSingleton<TService>(
            this IServiceContainer container,
            TService implementationInstance
        )
            where TService : class
        {
            return container.TryAddItem(ServiceDescriptor.Singleton(implementationInstance));
        }

        public static IServiceContainer TryAddSingleton(
            this IServiceContainer container,
            Type serviceType
        )
        {
            return container.TryAddItem(ServiceDescriptor.Singleton(serviceType, serviceType));
        }

        public static IServiceContainer TryAddSingleton(
            this IServiceContainer container,
            Type serviceType,
            Func<IServiceProvider, object> implementationFactory
        )
        {
            return container.TryAddItem(ServiceDescriptor.Singleton(serviceType, implementationFactory));
        }

        public static IServiceContainer TryAddSingleton(
            this IServiceContainer container,
            Type serviceType,
            Type implementationType
        )
        {
            return container.TryAddItem(ServiceDescriptor.Singleton(serviceType, implementationType));
        }

        public static IServiceContainer TryAddSingleton(
            this IServiceContainer container,
            Type serviceType,
            object implementationInstance
        )
        {
            return container.TryAddItem(ServiceDescriptor.Singleton(serviceType, implementationInstance));
        }

        #endregion
    }
}