using System;
using Annium.AspNetCore.Mesh;
using Annium.AspNetCore.Mesh.Internal.Middleware;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

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