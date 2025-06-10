using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Mesh.Transport.InMemory.Internal;

namespace Annium.Mesh.Transport.InMemory;

/// <summary>
/// Extension methods for registering in-memory mesh transport services
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Adds in-memory mesh transport services to the service container
    /// </summary>
    /// <param name="container">The service container to add services to</param>
    /// <returns>The service container for method chaining</returns>
    public static IServiceContainer AddMeshInMemoryTransport(this IServiceContainer container)
    {
        container.Add<ConnectionHub>().AsInterfaces().Singleton();
        container.Add<ClientConnectionFactory>().AsInterfaces().Singleton();

        return container;
    }
}
