using System;
using Annium.Logging.Abstractions;
using Annium.Storage.Abstractions;
using Annium.Storage.InMemory;
using Microsoft.Extensions.DependencyInjection;
using MemoryStorage = Annium.Storage.InMemory.Storage;

namespace Annium.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInMemoryStorage(this IServiceCollection services)
        {
            Func<IServiceProvider, Func<Configuration, MemoryStorage>> factory =
                sp => configuration => new MemoryStorage(configuration, sp.GetRequiredService<ILogger<MemoryStorage>>());

            services.AddSingleton<Func<Configuration, IStorage>>(factory);

            return services;
        }
    }
}