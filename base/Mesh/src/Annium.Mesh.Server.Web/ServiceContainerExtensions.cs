using Annium.Mesh.Server.Web.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddWebServerMeshHandler(this IServiceContainer container)
    {
        container.Add<Handler>().AsSelf().Singleton();

        return container;
    }
}
