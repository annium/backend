using Annium.Core.DependencyInjection;
using Annium.Mesh.Transport.InMemory;

namespace Annium.Mesh.Server.InMemory;

/// <summary>
/// Extension methods for registering in-memory mesh server services
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Adds in-memory mesh server services to the service container
    /// </summary>
    /// <param name="container">The service container to add services to</param>
    /// <returns>The service container for method chaining</returns>
    public static IServiceContainer AddMeshInMemoryServer(this IServiceContainer container)
    {
        container.AddMeshInMemoryTransport();

        container.Add<IServer, Internal.Server>().Singleton();

        return container;
    }
}
