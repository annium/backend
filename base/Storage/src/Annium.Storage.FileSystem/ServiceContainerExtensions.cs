using System;
using Annium.Logging.Abstractions;
using Annium.Storage.Abstractions;
using Annium.Storage.FileSystem;
using FsStorage = Annium.Storage.FileSystem.Storage;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddFileSystemStorage(this IServiceContainer container)
    {
        container.Add<Func<Configuration, IStorage>>(sp => configuration => new FsStorage(configuration, sp.Resolve<ILogger>())).AsSelf().Singleton();

        return container;
    }
}