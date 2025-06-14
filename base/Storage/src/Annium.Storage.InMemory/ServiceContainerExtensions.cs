using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Storage.Abstractions;

namespace Annium.Storage.InMemory;

/// <summary>
/// Extension methods for registering in-memory storage services in the service container
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Adds in-memory storage to the service container
    /// </summary>
    /// <param name="container">The service container to configure</param>
    /// <param name="key">The key to register the storage service under</param>
    /// <param name="isDefault">Whether this storage should be registered as the default implementation</param>
    /// <returns>The service container for method chaining</returns>
    public static IServiceContainer AddInMemoryStorage(
        this IServiceContainer container,
        object key,
        bool isDefault = false
    )
    {
        container.Add<IStorage>((sp, _) => new Internal.Storage(sp.Resolve<ILogger>())).AsKeyedSelf(key).Singleton();

        if (isDefault)
            container.Add<IStorage>(sp => sp.ResolveKeyed<IStorage>(key)).AsSelf().Singleton();

        return container;
    }
}
