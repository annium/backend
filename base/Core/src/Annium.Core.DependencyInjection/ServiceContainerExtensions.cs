using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceContainerExtensions
    {
        public static ServiceProvider BuildServiceProvider(this IServiceContainer container) =>
            container.Collection.BuildServiceProvider();
    }
}