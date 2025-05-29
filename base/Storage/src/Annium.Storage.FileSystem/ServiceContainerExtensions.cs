using System;
using Annium.Logging;
using Annium.Storage.Abstractions;
using Annium.Storage.FileSystem;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddFileSystemStorage(
        this IServiceContainer container,
        object key,
        Func<IServiceProvider, object?, Configuration> getConfiguration,
        bool isDefault = false
    )
    {
        container
            .Add<IStorage>(
                (sp, k) => new Storage.FileSystem.Internal.Storage(getConfiguration(sp, k), sp.Resolve<ILogger>())
            )
            .AsKeyedSelf(key)
            .Singleton();

        if (isDefault)
            container.Add<IStorage>(sp => sp.ResolveKeyed<IStorage>(key)).AsSelf().Singleton();

        return container;
    }
}
