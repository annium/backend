using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Descriptors;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Redis.Internal;

namespace Annium.Redis;

/// <summary>
/// Extension methods for configuring Redis services in the dependency injection container
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Adds Redis storage services to the container
    /// </summary>
    /// <param name="container">The service container to configure</param>
    /// <param name="lifetime">The service lifetime (default is Singleton)</param>
    /// <returns>The configured service container for chaining</returns>
    public static IServiceContainer AddRedis(
        this IServiceContainer container,
        ServiceLifetime lifetime = ServiceLifetime.Singleton
    )
    {
        container.Add<IRedisStorage, RedisStorage>().In(lifetime);

        return container;
    }
}
