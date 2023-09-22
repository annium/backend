using Annium.Cache.Abstractions;
using Annium.Cache.Redis.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddRedisCache(this IServiceContainer container, ServiceLifetime lifetime)
    {
        container.Add(typeof(Cache<,>)).As(typeof(ICache<,>)).In(lifetime);

        return container;
    }
}