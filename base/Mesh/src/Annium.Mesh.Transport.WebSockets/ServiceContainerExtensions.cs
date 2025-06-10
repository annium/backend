using System;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Mesh.Transport.WebSockets.Internal;

namespace Annium.Mesh.Transport.WebSockets;

/// <summary>
/// Extension methods for registering WebSocket-based mesh transport services
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Adds WebSocket-based server transport services to the service container
    /// </summary>
    /// <param name="container">The service container to add services to</param>
    /// <param name="getConfiguration">Function to retrieve server transport configuration</param>
    /// <returns>The service container for method chaining</returns>
    public static IServiceContainer AddMeshWebSocketsServerTransport(
        this IServiceContainer container,
        Func<IServiceProvider, ServerTransportConfiguration> getConfiguration
    )
    {
        container.Add<ServerConnectionFactory>().AsInterfaces().Singleton();
        container.Add(getConfiguration).AsSelf().Singleton();

        return container;
    }

    /// <summary>
    /// Adds WebSocket-based client transport services to the service container
    /// </summary>
    /// <param name="container">The service container to add services to</param>
    /// <param name="getConfiguration">Function to retrieve client transport configuration</param>
    /// <returns>The service container for method chaining</returns>
    public static IServiceContainer AddMeshWebSocketsClientTransport(
        this IServiceContainer container,
        Func<IServiceProvider, ClientTransportConfiguration> getConfiguration
    )
    {
        container.Add<ClientConnectionFactory>().AsInterfaces().Singleton();
        container.Add(getConfiguration).AsSelf().Singleton();

        return container;
    }
}
