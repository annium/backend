using Annium.Logging;
using Annium.Storage.Abstractions;
using Annium.Storage.InMemory.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddInMemoryStorage(
        this IServiceContainer container,
        object key,
        bool isDefault = false
    )
    {
        container.Add<IStorage>((sp, _) => new Storage.InMemory.Internal.Storage(sp.Resolve<ILogger>())).AsKeyedSelf(key).Singleton();

        if (isDefault)
            container.Add<IStorage>(sp => sp.ResolveKeyed<IStorage>(key)).AsSelf().Singleton();

        return container;
    }
}
