using System;
using Annium.Core.DependencyInjection;
using Annium.Mesh.Transport.WebSockets.Internal;

namespace Annium.Mesh.Transport.WebSockets;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddMeshWebSocketsServerTransport(
        this IServiceContainer container,
        Func<IServiceProvider, ServerTransportConfiguration> getConfiguration
    )
    {
        container.Add<IServerConnectionFactory, ServerConnectionFactory>().Singleton();
        container.Add(getConfiguration).AsSelf().Singleton();

        return container;
    }

    public static IServiceContainer AddMeshWebSocketsClientTransport(
        this IServiceContainer container,
        Func<IServiceProvider, ClientTransportConfiguration> getConfiguration
    )
    {
        container.Add<IClientConnectionFactory, ClientConnectionFactory>().Singleton();
        container.Add(getConfiguration).AsSelf().Singleton();

        return container;
    }
}