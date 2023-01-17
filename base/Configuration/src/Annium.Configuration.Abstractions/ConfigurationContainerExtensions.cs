using Annium.Configuration.Abstractions.Internal;

namespace Annium.Configuration.Abstractions;

public static class ConfigurationContainerExtensions
{
    public static IConfigurationContainer Add(
        this IConfigurationContainer container,
        object? config
    )
    {
        var configuration = new ObjectConfigurationProvider(config).Read();

        container.Add(configuration);

        return container;
    }
}