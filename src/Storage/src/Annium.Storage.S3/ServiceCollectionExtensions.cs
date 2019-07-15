using System;
using Annium.Logging.Abstractions;
using Annium.Storage.Abstractions;
using Annium.Storage.S3;
using Microsoft.Extensions.DependencyInjection;
using S3Storage = Annium.Storage.S3.Storage;

namespace Annium.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddS3Storage(this IServiceCollection services)
        {
            Func<IServiceProvider, Func<Configuration, S3Storage>> factory =
                sp => configuration => new S3Storage(configuration, sp.GetRequiredService<ILogger<S3Storage>>());

            services.AddSingleton<Func<Configuration, IStorage>>(factory);

            return services;
        }
    }
}