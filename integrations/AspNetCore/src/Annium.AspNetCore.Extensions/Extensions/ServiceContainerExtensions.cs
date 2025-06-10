using Annium.Architecture.Http.Profiles;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.Mapper;

namespace Annium.AspNetCore.Extensions.Extensions;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddAspNetCoreExtensions(this IServiceContainer container)
    {
        container.AddProfile<HttpStatusCodeProfile>();

        return container;
    }
}
