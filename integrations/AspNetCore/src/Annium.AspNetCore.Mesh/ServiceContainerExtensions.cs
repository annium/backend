using System;
using Annium.AspNetCore.Mesh.Internal.Middleware;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Extensions;

namespace Annium.AspNetCore.Mesh;

public static class ServiceContainerExtensions
{
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
