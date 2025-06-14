using Annium.Core.DependencyInjection;
using Annium.Mesh.Server.Sockets.Internal;

namespace Annium.Mesh.Server.Sockets;

/// <summary>
/// Provides extension methods for registering socket-based mesh server dependencies.
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Registers the socket server mesh handler as a singleton service in the container.
    /// </summary>
    /// <param name="container">The service container to configure.</param>
    /// <returns>The configured service container.</returns>
    public static IServiceContainer AddSocketServerMeshHandler(this IServiceContainer container)
    {
        container.Add<Handler>().AsSelf().Singleton();

        return container;
    }
}
