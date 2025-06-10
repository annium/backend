using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Mesh.Transport.InMemory.Internal;

namespace Annium.Mesh.Transport.InMemory;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddMeshInMemoryTransport(this IServiceContainer container)
    {
        container.Add<ConnectionHub>().AsInterfaces().Singleton();
        container.Add<ClientConnectionFactory>().AsInterfaces().Singleton();

        return container;
    }
}
