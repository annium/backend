using Annium.Redis;
using Annium.Redis.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddRedis(this IServiceContainer container, ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        container.Add<IRedisStorage, RedisStorage>().In(lifetime);

        return container;
    }
}