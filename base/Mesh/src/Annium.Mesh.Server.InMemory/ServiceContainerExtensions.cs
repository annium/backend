using Annium.Mesh.Server.InMemory;
using Annium.Mesh.Server.InMemory.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddMeshInMemoryServer(this IServiceContainer container)
    {
        container.AddMeshInMemoryTransport();

        container.Add<IServer, Server>().Singleton();

        return container;
    }
}
