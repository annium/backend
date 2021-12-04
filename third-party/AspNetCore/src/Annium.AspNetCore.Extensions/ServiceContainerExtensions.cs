using Annium.Architecture.Http.Profiles;

namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddAspNetCoreExtensions(
        this IServiceContainer container
    )
    {
        container.AddProfile<HttpStatusCodeProfile>();

        return container;
    }
}