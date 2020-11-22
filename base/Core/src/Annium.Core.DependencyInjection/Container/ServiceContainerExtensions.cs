using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceContainerExtensions
    {
        public static ISingleRegistrationBuilderBase Add<TImplementationType>(this IServiceContainer container) =>
            container.Add(typeof(TImplementationType));

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