using System;
using Annium.Logging.Abstractions;
using Annium.Storage.Abstractions;
using Annium.Storage.FileSystem;
using FsStorage = Annium.Storage.FileSystem.Storage;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceContainer AddFileSystemStorage(this IServiceContainer container)
        {
            Func<IServiceProvider, Func<Configuration, FsStorage>> factory =
                sp => configuration => new FsStorage(configuration, sp.Resolve<ILogger<FsStorage>>());

            container.Add<Func<Configuration, IStorage>>(factory).Singleton();

            return container;
        }
    }
}