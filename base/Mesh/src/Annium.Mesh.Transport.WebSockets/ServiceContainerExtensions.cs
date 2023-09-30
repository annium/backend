using Annium.Core.DependencyInjection;
using Annium.Mesh.Transport.WebSockets.Internal;

namespace Annium.Mesh.Transport.WebSockets;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddMeshSocketsTransport(this IServiceContainer container)
    {
        container.Add<IClientConnectionFactory, ClientConnectionFactory>().Singleton();
        container.Add<IServerConnectionFactory, ServerConnectionFactory>().Singleton();

        return container;
    }
}