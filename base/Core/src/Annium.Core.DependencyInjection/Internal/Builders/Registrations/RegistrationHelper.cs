using System;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations
{
    internal static class RegistrationHelper
    {
        /// <summary>
        /// Create T factory like:
        /// <example>
        /// (IServiceProvider sp) => sp.GetRequiredService()
        /// </example>
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="implementationType"></param>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        public static IServiceDescriptor CreateTypeFactoryDescriptor(
            Type serviceType,
            Type implementationType,
            ServiceLifetime lifetime
        )
        {
            var (getService, sp) = CreateFactoryParts(implementationType);
            var factory = Expression.Lambda<Func<IServiceProvider, object>>(getService, sp).Compile();

            return ServiceDescriptor.Factory(serviceType, factory, lifetime);
        }

        /// <summary>
        /// Create Func(T) factory like:
        /// <example>
        /// (IServiceProvider sp) => () => sp.GetRequiredService()
        /// </example>
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="implementationType"></param>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        public static IServiceDescriptor CreateFuncFactoryDescriptor(
            Type serviceType,
            Type implementationType,
            ServiceLifetime lifetime
        )
        {
            var factoryServiceType = typeof(Func<>).MakeGenericType(serviceType);
            var (getService, sp) = CreateFactoryParts(implementationType);
            var factory = Expression.Lambda<Func<IServiceProvider, object>>(Expression.Lambda(getService), sp).Compile();

            return ServiceDescriptor.Factory(factoryServiceType, factory, lifetime);
        }

        /// <summary>
        /// Create (TKey,T) key factory like:
        /// <example>
        /// (IServiceProvider sp) => new KeyValue(key, sp.GetRequiredService())
        /// </example>
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="implementationType"></param>
        /// <param name="keyType"></param>
        /// <param name="key"></param>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        public static IServiceDescriptor CreateTypeKeyFactoryDescriptor(
            Type serviceType,
            Type implementationType,
            Type keyType,
            object key,
            ServiceLifetime lifetime
        )
        {
            var keyServiceType = typeof(KeyValue<,>).MakeGenericType(keyType, serviceType);
            var (getService, sp) = CreateFactoryParts(implementationType);
            var factory = Expression.Lambda<Func<IServiceProvider, object>>(
                Expression.New(
                    typeof(KeyValue<,>).MakeGenericType(keyType, serviceType).GetConstructor(new[] { keyType, serviceType })!,
                    Expression.Constant(key),
                    getService
                ),
                sp
            ).Compile();

            return ServiceDescriptor.Factory(keyServiceType, factory, lifetime);
        }

        /// <summary>
        /// Create (TKey,Func(T)) key factory like:
        /// <example>
        /// (IServiceProvider sp) => new KeyValue(key, () => sp.GetRequiredService())
        /// </example>
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="implementationType"></param>
        /// <param name="keyType"></param>
        /// <param name="key"></param>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        public static IServiceDescriptor CreateFuncKeyFactoryDescriptor(
            Type serviceType,
            Type implementationType,
            Type keyType,
            object key,
            ServiceLifetime lifetime
        )
        {
            var keyServiceType = typeof(KeyValue<,>).MakeGenericType(keyType, typeof(Func<>).MakeGenericType(serviceType));
            var (getService, sp) = CreateFactoryParts(implementationType);
            var factory = Expression.Lambda<Func<IServiceProvider, object>>(
                Expression.New(
                    typeof(KeyValue<,>).MakeGenericType(keyType, serviceType).GetConstructor(new[] { keyType, serviceType })!,
                    Expression.Constant(key),
                    Expression.Lambda(getService)
                ),
                sp
            ).Compile();

            return ServiceDescriptor.Factory(keyServiceType, factory, lifetime);
        }

        /// <summary>
        /// Create instance getter like:
        /// <example>
        /// (IServiceProvider sp) => instance
        /// </example>
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="instance"></param>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        public static IServiceDescriptor CreateInstanceFactoryDescriptor(
            Type serviceType,
            object instance,
            ServiceLifetime lifetime
        )
        {
            var sp = Expression.Parameter(typeof(IServiceProvider));
            var factory = Expression.Lambda<Func<IServiceProvider, object>>(Expression.Constant(instance), sp).Compile();

            return ServiceDescriptor.Factory(serviceType, factory, lifetime);
        }

        /// <summary>
        /// Create instance key getter like:
        /// <example>
        /// (IServiceProvider sp) => new KeyValue(key, instance)
        /// </example>
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="instance"></param>
        /// <param name="keyType"></param>
        /// <param name="key"></param>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        public static IServiceDescriptor CreateInstanceKeyFactoryDescriptor(
            Type serviceType,
            object instance,
            Type keyType,
            object key,
            ServiceLifetime lifetime
        )
        {
            var keyServiceType = typeof(KeyValue<,>).MakeGenericType(keyType, serviceType);
            var sp = Expression.Parameter(typeof(IServiceProvider));
            var factory = Expression.Lambda<Func<IServiceProvider, object>>(
                Expression.New(
                    typeof(KeyValue<,>).MakeGenericType(keyType, serviceType).GetConstructor(new[] { keyType, serviceType })!,
                    Expression.Constant(key),
                    Expression.Constant(instance)
                ),
                sp
            ).Compile();

            return ServiceDescriptor.Factory(keyServiceType, factory, lifetime);
        }

        /// <summary>
        /// Creates factory parts:
        /// - IServiceProvider.GetRequiredService() call as lambda body
        /// - IServiceProvider parameter, used for call as ParameterExpression
        /// </summary>
        /// <param name="implementationType"></param>
        /// <returns></returns>
        private static (Expression, ParameterExpression) CreateFactoryParts(Type implementationType)
        {
            var sp = Expression.Parameter(typeof(IServiceProvider));
            var getService = typeof(ServiceProviderServiceExtensions)
                .GetMethod(nameof(ServiceProviderServiceExtensions.GetRequiredService), new[] { typeof(IServiceProvider), typeof(Type) })!;
            var typeArg = Expression.Constant(implementationType);
            var body = Expression.Convert(Expression.Call(null, getService, sp, typeArg), implementationType);

            return (body, sp);
        }
    }
}