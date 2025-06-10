using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Mesh.Transport.InMemory;

namespace Annium.Mesh.Server.InMemory;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddMeshInMemoryServer(this IServiceContainer container)
    {
        container.AddMeshInMemoryTransport();

        container.Add<IServer, Internal.Server>().Singleton();

        return container;
    }
}
