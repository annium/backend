using Annium.Core.DependencyInjection.Builders;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.Runtime;
using Annium.linq2db.Extensions.Configuration;

namespace Annium.linq2db.Extensions;

public static class ServiceContainerExtensions
{
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
