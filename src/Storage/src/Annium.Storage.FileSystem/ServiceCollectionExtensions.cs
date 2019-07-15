using System;
using Annium.Logging.Abstractions;
using Annium.Storage.Abstractions;
using Annium.Storage.FileSystem;
using Microsoft.Extensions.DependencyInjection;
using FsStorage = Annium.Storage.FileSystem.Storage;

namespace Annium.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFileSystemStorage(this IServiceCollection services)
        {
            Func<IServiceProvider, Func<Configuration, FsStorage>> factory =
                sp => configuration => new FsStorage(configuration, sp.GetRequiredService<ILogger<FsStorage>>());

            services.AddSingleton<Func<Configuration, IStorage>>(factory);

            return services;
        }
    }
}