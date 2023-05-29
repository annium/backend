using Annium.Cache.Abstractions;
using Annium.Cache.InMemory.Internal;
using Annium.Core.DependencyInjection;

namespace Annium.Cache.InMemory;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddInMemoryCache(this IServiceContainer container, ServiceLifetime lifetime)
    {
        container.Add(typeof(Cache<,>)).As(typeof(ICache<,>)).In(lifetime);

        return container;
    }
}