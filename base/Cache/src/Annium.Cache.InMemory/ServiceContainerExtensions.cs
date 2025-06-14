using Annium.Cache.Abstractions;
using Annium.Cache.InMemory.Internal;
using Annium.Core.DependencyInjection;

namespace Annium.Cache.InMemory;

/// <summary>
/// Extension methods for registering in-memory cache services
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Registers in-memory cache implementation in the service container
    /// </summary>
    /// <param name="container">The service container</param>
    /// <param name="lifetime">The service lifetime for cache instances</param>
    /// <returns>The service container for chaining</returns>
    public static IServiceContainer AddInMemoryCache(this IServiceContainer container, ServiceLifetime lifetime)
    {
        container.Add(typeof(Cache<,>)).As(typeof(ICache<,>)).In(lifetime);

        return container;
    }
}
