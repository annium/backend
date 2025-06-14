using Annium.Core.DependencyInjection;
using Annium.Mesh.Client.Internal;

namespace Annium.Mesh.Client;

/// <summary>
/// Extension methods for registering mesh client services in the dependency injection container
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Registers mesh client services in the dependency injection container
    /// </summary>
    /// <param name="container">The service container to register services in</param>
    /// <returns>The service container for fluent chaining</returns>
    public static IServiceContainer AddMeshClient(this IServiceContainer container)
    {
        // public
        container.Add<IClientFactory, ClientFactory>().Singleton();

        return container;
    }
}
