using Annium.Architecture.Http.Profiles;

// ReSharper disable once CheckNamespace
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