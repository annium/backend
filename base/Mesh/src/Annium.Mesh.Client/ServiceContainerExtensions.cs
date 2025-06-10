using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Mesh.Client.Internal;

namespace Annium.Mesh.Client;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddMeshClient(this IServiceContainer container)
    {
        // public
        container.Add<IClientFactory, ClientFactory>().Singleton();

        return container;
    }
}
