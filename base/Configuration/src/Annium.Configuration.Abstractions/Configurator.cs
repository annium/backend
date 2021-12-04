using System;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;

namespace Annium.Configuration.Abstractions;

public static class Configurator
{
    public static T Get<T>(
        Action<IConfigurationContainer> configure,
        bool tryLoadReferences
    )
        where T : class, new()
    {
        var container = GetServices<T>(tryLoadReferences);

        container.AddConfiguration<T>(configure);

        return Get<T>(container);
    }

    public static async Task<T> Get<T>(
        Func<IConfigurationContainer, Task> configure,
        bool tryLoadReferences
    )
        where T : class, new()
    {
        var container = GetServices<T>(tryLoadReferences);

        await container.AddConfiguration<T>(configure);

        return Get<T>(container);
    }

    private static IServiceContainer GetServices<T>(bool tryLoadReferences)
    {
        var container = new ServiceContainer();

        container.AddRuntimeTools(typeof(T).Assembly, tryLoadReferences);
        container.AddMapper();

        return container;
    }

    private static T Get<T>(IServiceContainer container)
        where T : notnull
    {
        var provider = container.BuildServiceProvider();

        var configuration = provider.Resolve<T>();

        return configuration;
    }
}