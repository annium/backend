using Annium.Serialization.Abstractions;
using Annium.Serialization.Abstractions.Internal;

namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static ISerializationConfigurationBuilder AddSerializers(
        this IServiceContainer container
    ) => new SerializationConfigurationBuilder(container, Constants.DefaultKey);

    public static ISerializationConfigurationBuilder AddSerializers(
        this IServiceContainer container,
        string key
    ) => new SerializationConfigurationBuilder(container, key);
}