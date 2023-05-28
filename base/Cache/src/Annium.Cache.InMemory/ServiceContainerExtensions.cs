using Annium.Core.DependencyInjection;

namespace Annium.Cache.InMemory;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddInMemoryCache(this IServiceContainer container)
    {
        return container;
    }
}