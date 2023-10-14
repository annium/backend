using Annium.Mesh.Transport.InMemory.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddMeshInMemoryTransport(
        this IServiceContainer container
    )
    {
        container.Add<ConnectionHub>().AsInterfaces().Singleton();
        container.Add<ClientConnectionFactory>().AsInterfaces().Singleton();

        return container;
    }
}