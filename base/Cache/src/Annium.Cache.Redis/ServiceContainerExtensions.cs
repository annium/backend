using Annium.Cache.Abstractions;
using Annium.Cache.Redis.Internal;
using Annium.Core.DependencyInjection;

namespace Annium.Cache.Redis;

/// <summary>
/// Extension methods for registering Redis cache services
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Registers Redis cache implementation in the service container
    /// </summary>
    /// <param name="container">The service container</param>
    /// <param name="lifetime">The service lifetime for cache instances</param>
    /// <returns>The service container for chaining</returns>
    public static IServiceContainer AddRedisCache(this IServiceContainer container, ServiceLifetime lifetime)
    {
        container.Add(typeof(Cache<,>)).As(typeof(ICache<,>)).In(lifetime);

        return container;
    }
}
