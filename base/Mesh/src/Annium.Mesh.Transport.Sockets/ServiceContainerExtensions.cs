using System;
using Annium.Core.DependencyInjection;
using Annium.Mesh.Transport.Sockets.Internal;

namespace Annium.Mesh.Transport.Sockets;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddMeshSocketsServerTransport(
        this IServiceContainer container,
        Func<IServiceProvider, ServerTransportConfiguration> getConfiguration
    )
    {
        container.Add<ServerConnectionFactory>().AsInterfaces().Singleton();
        container.Add(getConfiguration).AsSelf().Singleton();

        return container;
    }

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