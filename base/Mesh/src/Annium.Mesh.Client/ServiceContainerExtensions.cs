using Annium.Mesh.Client;
using Annium.Mesh.Client.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddMeshClient(this IServiceContainer container)
    {
        // public
        container.Add<IClientFactory, ClientFactory>().Singleton();

        return container;
    }
}