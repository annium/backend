using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Mesh.Server.Web.Internal;

namespace Annium.Mesh.Server.Web;

/// <summary>
/// Provides extension methods for registering web-based mesh server dependencies.
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Registers the web server mesh handler as a singleton service in the container.
    /// </summary>
    /// <param name="container">The service container to configure.</param>
    /// <returns>The configured service container.</returns>
    public static IServiceContainer AddWebServerMeshHandler(this IServiceContainer container)
    {
        container.Add<Handler>().AsSelf().Singleton();

        return container;
    }
}
