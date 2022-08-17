using System;
using Annium.Logging.Abstractions;
using Annium.Storage.Abstractions;
using Annium.Storage.InMemory;
using MemoryStorage = Annium.Storage.InMemory.Storage;

namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddInMemoryStorage(this IServiceContainer container)
    {
        container.Add<Func<Configuration, IStorage>>(sp => _ => new MemoryStorage(sp.Resolve<ILogger<MemoryStorage>>())).AsSelf().Singleton();

        return container;
    }
}