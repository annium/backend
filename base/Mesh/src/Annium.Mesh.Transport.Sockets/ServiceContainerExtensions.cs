using System;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Mesh.Transport.Sockets.Internal;

namespace Annium.Mesh.Transport.Sockets;

/// <summary>
/// Extension methods for registering socket-based mesh transport services
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Adds socket-based server transport services to the service container
    /// </summary>
    /// <param name="container">The service container to add services to</param>
    /// <param name="getConfiguration">Function to retrieve server transport configuration</param>
    /// <returns>The service container for method chaining</returns>
    public static IServiceContainer AddMeshSocketsServerTransport(
        this IServiceContainer container,
        Func<IServiceProvider, ServerTransportConfiguration> getConfiguration
    )
    {
        container.Add<ServerConnectionFactory>().AsInterfaces().Singleton();
        container.Add(getConfiguration).AsSelf().Singleton();

        return container;
    }

    /// <summary>
    /// Adds socket-based client transport services to the service container
    /// </summary>
    /// <param name="container">The service container to add services to</param>
    /// <param name="getConfiguration">Function to retrieve client transport configuration</param>
    /// <returns>The service container for method chaining</returns>
    public static IServiceContainer AddMeshSocketsClientTransport(
        this IServiceContainer container,
        Func<IServiceProvider, ClientTransportConfiguration> getConfiguration
    )
    {
        container.Add<ClientConnectionFactory>().AsInterfaces().Singleton();
        container.Add(getConfiguration).AsSelf().Singleton();

        return container;
    }
}
