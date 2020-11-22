using System;
using Annium.Logging.Abstractions;
using Annium.Storage.Abstractions;
using Annium.Storage.InMemory;
using MemoryStorage = Annium.Storage.InMemory.Storage;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceContainer AddInMemoryStorage(this IServiceContainer container)
        {
            Func<IServiceProvider, Func<Configuration, MemoryStorage>> factory =
                sp => configuration => new MemoryStorage(configuration, sp.Resolve<ILogger<MemoryStorage>>());

            container.Add<Func<Configuration, IStorage>>(factory).Singleton();

            return container;
        }
    }
}