using Annium.linq2db.Extensions;

namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddRepositories(
        this IServiceContainer container
    )
    {
        container.AddAll()
            .AssignableTo<IRepository>()
            .AsInterfaces()
            .Scoped();

        return container;
    }
}