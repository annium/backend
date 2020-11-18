using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static partial class ServiceContainerExtensions
    {
        public static ServiceProvider BuildServiceProvider(this IServiceContainer container) =>
            container.Collection.BuildServiceProvider();
    }
}