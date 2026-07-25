using System;
using Annium.Cache.Abstractions;
using Annium.Cache.Redis.Internal;
using Annium.Core.DependencyInjection;

namespace Annium.Cache.Redis;

/// <summary>
/// Extension methods for registering the Redis cache implementation.
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Registers the Redis cache implementation in the service container. The underlying Redis
    /// connection is expected to be registered separately via <c>AddRedis</c> (an
    /// <c>IRedisStorage</c> singleton).
    /// </summary>
    /// <param name="container">The service container.</param>
    /// <param name="configure">Optional callback to configure <see cref="RedisCacheOptions"/> (e.g. the key prefix).</param>
    /// <param name="lifetime">The service lifetime for cache instances.</param>
    /// <returns>The service container for chaining.</returns>
    public static IServiceContainer AddRedisCache(
        this IServiceContainer container,
        Action<RedisCacheOptions>? configure = null,
        ServiceLifetime lifetime = ServiceLifetime.Singleton
    )
    {
        var options = new RedisCacheOptions();
        configure?.Invoke(options);
        container.Add(options).AsSelf().Singleton();

        container.Add(typeof(Cache<,>)).As(typeof(ICache<,>)).In(lifetime);

        return container;
    }
}
