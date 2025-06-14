using Annium.Architecture.Http.Profiles;
using Annium.Core.DependencyInjection;
using Annium.Core.Mapper;

namespace Annium.AspNetCore.Extensions.Extensions;

/// <summary>
/// Extension methods for configuring ASP.NET Core extensions in the service container
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Adds ASP.NET Core extensions including HTTP status code mapping profiles
    /// </summary>
    /// <param name="container">The service container to configure</param>
    /// <returns>The configured service container</returns>
    public static IServiceContainer AddAspNetCoreExtensions(this IServiceContainer container)
    {
        container.AddProfile<HttpStatusCodeProfile>();

        return container;
    }
}
