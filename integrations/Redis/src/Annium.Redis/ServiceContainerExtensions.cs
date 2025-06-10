using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Descriptors;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Redis.Internal;

namespace Annium.Redis;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddRedis(
        this IServiceContainer container,
        ServiceLifetime lifetime = ServiceLifetime.Singleton
    )
    {
        container.Add<IRedisStorage, RedisStorage>().In(lifetime);

        return container;
    }
}
