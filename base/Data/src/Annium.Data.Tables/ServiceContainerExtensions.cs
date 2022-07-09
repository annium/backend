using Annium.Data.Tables;
using Annium.Data.Tables.Internal;

namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddTables(
        this IServiceContainer container
    )
    {
        container.Add<ITableFactory, TableFactory>().Singleton();

        return container;
    }
}