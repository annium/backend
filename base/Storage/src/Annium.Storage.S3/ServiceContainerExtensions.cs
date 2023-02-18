using System;
using Annium.Logging.Abstractions;
using Annium.Storage.Abstractions;
using Annium.Storage.S3;
using S3Storage = Annium.Storage.S3.Storage;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddS3Storage(this IServiceContainer container)
    {
        container.Add<Func<Configuration, IStorage>>(sp => configuration => new S3Storage(configuration, sp.Resolve<ILogger<S3Storage>>())).AsSelf().Singleton();

        return container;
    }
}