using System;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Mesh.Transport.WebSockets.Internal;

namespace Annium.Mesh.Transport.WebSockets;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddMeshWebSocketsServerTransport(
        this IServiceContainer container,
        Func<IServiceProvider, ServerTransportConfiguration> getConfiguration
    )
    {
        container.Add<ServerConnectionFactory>().AsInterfaces().Singleton();
        container.Add(getConfiguration).AsSelf().Singleton();

        return container;
    }

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
