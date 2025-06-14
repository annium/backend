using System;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Storage.Abstractions;

namespace Annium.Storage.FileSystem;

/// <summary>
/// Extension methods for registering file system storage services in the service container
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Adds file system storage to the service container with the specified configuration
    /// </summary>
    /// <param name="container">The service container to configure</param>
    /// <param name="key">The key to register the storage service under</param>
    /// <param name="getConfiguration">Function to retrieve the storage configuration</param>
    /// <param name="isDefault">Whether this storage should be registered as the default implementation</param>
    /// <returns>The service container for method chaining</returns>
    public static IServiceContainer AddFileSystemStorage(
        this IServiceContainer container,
        object key,
        Func<IServiceProvider, object?, Configuration> getConfiguration,
        bool isDefault = false
    )
    {
        container
            .Add<IStorage>((sp, k) => new Internal.Storage(getConfiguration(sp, k), sp.Resolve<ILogger>()))
            .AsKeyedSelf(key)
            .Singleton();

        if (isDefault)
            container.Add<IStorage>(sp => sp.ResolveKeyed<IStorage>(key)).AsSelf().Singleton();

        return container;
    }
}
