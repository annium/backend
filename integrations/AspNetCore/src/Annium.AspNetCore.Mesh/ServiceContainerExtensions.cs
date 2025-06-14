using System;
using Annium.AspNetCore.Mesh.Internal.Middleware;
using Annium.Core.DependencyInjection;

namespace Annium.AspNetCore.Mesh;

/// <summary>
/// Extension methods for configuring Mesh WebSockets middleware in the service container
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Adds Mesh WebSockets middleware to the service container with the specified configuration
    /// </summary>
    /// <param name="container">The service container to configure</param>
    /// <param name="configure">Action to configure the WebSockets middleware</param>
    /// <returns>The configured service container</returns>
    public static IServiceContainer AddMeshWebSocketsMiddleware(
        this IServiceContainer container,
        Action<WebSocketsMiddlewareConfiguration> configure
    )
    {
        container.Add<WebSocketsMiddleware>().AsSelf().Singleton();

        var config = new WebSocketsMiddlewareConfiguration();
        configure(config);

        container.Add(config).AsSelf().Singleton();

        return container;
    }
}
