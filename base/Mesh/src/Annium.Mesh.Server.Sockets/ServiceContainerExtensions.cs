using Annium.Mesh.Server.Sockets.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddSocketServerMeshHandler(this IServiceContainer container)
    {
        container.Add<Handler>().AsSelf().Singleton();

        return container;
    }
}