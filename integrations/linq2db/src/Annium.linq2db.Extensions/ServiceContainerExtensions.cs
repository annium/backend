using Annium.linq2db.Extensions.Configuration;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddEntityConfigurations(
        this IServiceContainer container
    )
    {
        container.AddAll()
            .Where(x => x is { IsClass: true, IsAbstract: false })
            .AssignableTo(typeof(IEntityConfiguration<>))
            .As<IEntityConfiguration>()
            .Singleton();

        return container;
    }
}