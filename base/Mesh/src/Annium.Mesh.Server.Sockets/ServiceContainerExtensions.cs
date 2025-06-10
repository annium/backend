using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Mesh.Server.Sockets.Internal;

namespace Annium.Mesh.Server.Sockets;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddSocketServerMeshHandler(this IServiceContainer container)
    {
        container.Add<Handler>().AsSelf().Singleton();

        return container;
    }
}
