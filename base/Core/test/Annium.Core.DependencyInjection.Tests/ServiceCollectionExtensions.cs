using System;
using System.Linq;
using Annium.Core.Primitives;
using Annium.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection.Tests
{
    internal static class ServiceCollectionExtensions
    {
        public static void Has(
            this IServiceCollection services,
            int count
        )
        {
            services.Count.IsEqual(count, $"Expected to have {count} services registered, but found {services.Count}.");
        }

        public static void Has(
            this IServiceCollection services,
            Type serviceType,
            int count
        )
        {
            var descriptors = services.Where(x => x.ServiceType == serviceType).ToArray();
            descriptors.Length.IsEqual(count, $"Expected to have {count} {serviceType.FriendlyName()} services registered, but found {descriptors.Length}.");
        }

        public static void HasScoped(this IServiceCollection services, Type serviceType, Type implementationType) =>
            services.Has(serviceType, implementationType, ServiceLifetime.Scoped);

        public static void HasSingleton(this IServiceCollection services, Type serviceType, Type implementationType) =>
            services.Has(serviceType, implementationType, ServiceLifetime.Singleton);

        public static void HasTransient(this IServiceCollection services, Type serviceType, Type implementationType) =>
            services.Has(serviceType, implementationType, ServiceLifetime.Transient);

        public static void HasScopedTypeFactory(this IServiceCollection services, Type serviceType, int count = 1) =>
            services.HasTypeFactory(serviceType, ServiceLifetime.Scoped, count);

        public static void HasSingletonTypeFactory(this IServiceCollection services, Type serviceType, int count = 1) =>
            services.HasTypeFactory(serviceType, ServiceLifetime.Singleton, count);

        public static void HasTransientTypeFactory(this IServiceCollection services, Type serviceType, int count = 1) =>
            services.HasTypeFactory(serviceType, ServiceLifetime.Transient, count);

        public static void HasScopedFuncFactory(this IServiceCollection services, Type serviceType, int count = 1) =>
            services.HasFuncFactory(serviceType, ServiceLifetime.Scoped, count);

        public static void HasSingletonFuncFactory(this IServiceCollection services, Type serviceType, int count = 1) =>
            services.HasFuncFactory(serviceType, ServiceLifetime.Singleton, count);

        public static void HasTransientFuncFactory(this IServiceCollection services, Type serviceType, int count = 1) =>
            services.HasFuncFactory(serviceType, ServiceLifetime.Transient, count);

        private static void Has(this IServiceCollection services, Type serviceType, Type implementationType, ServiceLifetime lifetime)
        {
            var descriptors = services.GetDescriptors(serviceType);
            var descriptor = descriptors.SingleOrDefault(x => x.ImplementationType == implementationType);
            descriptor.IsNotDefault($"Not found {serviceType.FriendlyName()} -> {implementationType.FriendlyName()} descriptor");
            descriptor!.Lifetime.IsEqual(
                lifetime,
                $"Descriptor {serviceType.FriendlyName()} -> {implementationType.FriendlyName()} is {descriptor.Lifetime}, but {lifetime} expected."
            );
        }

        private static void HasTypeFactory(this IServiceCollection services, Type serviceType, ServiceLifetime lifetime, int count)
        {
            var descriptors = services
                .GetDescriptors(serviceType)
                .Where(x => x.ImplementationFactory != null)
                .ToArray();
            descriptors.Length.IsEqual(count, $"Expected {count} {serviceType.FriendlyName()} descriptors, but found {descriptors.Length}");
            foreach (var descriptor in descriptors)
                descriptor.Lifetime.IsEqual(
                    lifetime,
                    $"Descriptor {serviceType.FriendlyName()} is {descriptor.Lifetime}, but {lifetime} expected."
                );
        }

        private static void HasFuncFactory(this IServiceCollection services, Type serviceType, ServiceLifetime lifetime, int count)
        {
            var descriptors = services
                .GetDescriptors(typeof(Func<>).MakeGenericType(serviceType))
                .Where(x => x.ImplementationFactory != null)
                .ToArray();
            descriptors.Length.IsEqual(count, $"Expected {count} () => {serviceType.FriendlyName()} descriptors, but found {descriptors.Length}");
            foreach (var descriptor in descriptors)
                descriptor.Lifetime.IsEqual(
                    lifetime,
                    $"Descriptor () => {serviceType.FriendlyName()} is {descriptor.Lifetime}, but {lifetime} expected."
                );
        }

        private static ServiceDescriptor[] GetDescriptors(
            this IServiceCollection services,
            Type serviceType
        )
        {
            var descriptors = services.Where(x => x.ServiceType == serviceType).ToArray();
            descriptors.Length.IsNotDefault($"No {serviceType.FriendlyName()} based descriptors found");

            return descriptors;
        }
    }
}