using Annium.Core.DependencyInjection;
using Annium.Mesh.Transport.Sockets.Internal;

namespace Annium.Mesh.Transport.Sockets;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddMeshSocketsTransport(this IServiceContainer container)
    {
        container.Add<IClientConnectionFactory, ClientConnectionFactory>().Singleton();
        container.Add<IServerConnectionFactory, ServerConnectionFactory>().Singleton();

        return container;
    }
}