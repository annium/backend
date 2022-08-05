using Annium.linq2db.Extensions;
using Annium.linq2db.Extensions.Configuration;

namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddRepositories(
        this IServiceContainer container
    )
    {
        container.AddAll()
            .Where(x => x.IsClass && !x.IsAbstract)
            .AssignableTo<IRepository>()
            .AsInterfaces()
            .Scoped();

        return container;
    }

    public static IServiceContainer AddEntityConfigurations(
        this IServiceContainer container
    )
    {
        container.AddAll()
            .Where(x => x.IsClass && !x.IsAbstract)
            .AssignableTo(typeof(IEntityConfiguration<>))
            .As<IEntityConfiguration>()
            .Singleton();

        return container;
    }
}