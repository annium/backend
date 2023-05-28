using Annium.Core.DependencyInjection;

namespace Annium.Cache.Redis;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddInMemoryCache(this IServiceContainer container)
    {
        return container;
    }
}