using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Logging;
using Annium.Storage.Abstractions;

namespace Annium.Storage.InMemory;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddInMemoryStorage(
        this IServiceContainer container,
        object key,
        bool isDefault = false
    )
    {
        container.Add<IStorage>((sp, _) => new Internal.Storage(sp.Resolve<ILogger>())).AsKeyedSelf(key).Singleton();

        if (isDefault)
            container.Add<IStorage>(sp => sp.ResolveKeyed<IStorage>(key)).AsSelf().Singleton();

        return container;
    }
}
