using System;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceContainerExtensions
    {
        public static IFactoryRegistrationBuilderBase Add<T>(this IServiceContainer container, Func<IServiceProvider, T> factory) where T : class =>
            container.Add(typeof(T), factory);

        public static ISingleRegistrationBuilderBase Add<TService, TImplementation>(this IServiceContainer container) where TImplementation : TService =>
            container.Add(typeof(TImplementation)).As<TService>();

        public static ISingleRegistrationBuilderBase Add<TImplementationType>(this IServiceContainer container) =>
            container.Add(typeof(TImplementationType));

        public static IServiceContainer Clone(this IServiceContainer container)
        {
            var clone = new ServiceContainer();

            foreach (var descriptor in container)
                clone.Add(descriptor);

            return clone;
        }
    }
}