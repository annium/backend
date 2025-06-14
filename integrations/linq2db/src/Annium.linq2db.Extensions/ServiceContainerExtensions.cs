using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;

namespace Annium.linq2db.Extensions;

/// <summary>
/// Extension methods for configuring linq2db services in the dependency injection container
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Registers all entity configurations found in the application assemblies
    /// </summary>
    /// <param name="container">The service container to configure</param>
    /// <returns>The configured service container for chaining</returns>
    public static IServiceContainer AddEntityConfigurations(this IServiceContainer container)
    {
        container
            .AddAll()
            .Where(x => x is { IsClass: true, IsAbstract: false })
            .AssignableTo(typeof(IEntityConfiguration<>))
            .As<IEntityConfiguration>()
            .Singleton();

        return container;
    }
}
