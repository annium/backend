using System;
using Annium.Logging.Abstractions;
using Annium.Storage.Abstractions;
using Annium.Storage.S3;
using S3Storage = Annium.Storage.S3.Storage;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceContainer AddS3Storage(this IServiceContainer container)
        {
            Func<IServiceProvider, Func<Configuration, S3Storage>> factory =
                sp => configuration => new S3Storage(configuration, sp.Resolve<ILogger<S3Storage>>());

            container.Add<Func<Configuration, IStorage>>(factory).AsSelf().Singleton();

            return container;
        }
    }
}