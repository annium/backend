using System;
using Annium.Mesh.Transport.Sockets;
using Annium.Mesh.Transport.Sockets.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

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