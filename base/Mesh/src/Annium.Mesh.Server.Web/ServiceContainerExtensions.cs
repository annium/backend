using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Mesh.Server.Web.Internal;

namespace Annium.Mesh.Server.Web;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddWebServerMeshHandler(this IServiceContainer container)
    {
        container.Add<Handler>().AsSelf().Singleton();

        return container;
    }
}
