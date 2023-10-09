using System;
using System.Net.Sockets;
using Annium.Core.DependencyInjection;
using Annium.Mesh.Transport.Abstractions;
using Annium.Mesh.Transport.Sockets.Internal;

namespace Annium.Mesh.Transport.Sockets;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddMeshSocketsServerTransport(
        this IServiceContainer container,
        Func<IServiceProvider, ServerTransportConfiguration> getConfiguration
    )
    {
        container.Add<IServerConnectionFactory<Socket>, ServerConnectionFactory>().Singleton();
        container.Add(getConfiguration).AsSelf().Singleton();

        return container;
    }

    public static IServiceContainer AddMeshSocketsClientTransport(
        this IServiceContainer container,
        Func<IServiceProvider, ClientTransportConfiguration> getConfiguration
    )
    {
        container.Add<IClientConnectionFactory, ClientConnectionFactory>().Singleton();
        container.Add(getConfiguration).AsSelf().Singleton();

        return container;
    }
}