using Annium.Cache.Abstractions;
using Annium.Cache.Redis.Internal;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Descriptors;

namespace Annium.Cache.Redis;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddRedisCache(this IServiceContainer container, ServiceLifetime lifetime)
    {
        container.Add(typeof(Cache<,>)).As(typeof(ICache<,>)).In(lifetime);

        return container;
    }
}
