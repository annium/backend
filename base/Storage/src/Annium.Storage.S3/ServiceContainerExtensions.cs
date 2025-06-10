using System;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Logging;
using Annium.Storage.Abstractions;

namespace Annium.Storage.S3;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddS3Storage(
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
