using Annium.Serialization.Json;
using Annium.Serialization.Json.Internal;
using Constants = Annium.Serialization.Abstractions.Constants;

namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IJsonSerializersConfigurationBuilder AddJsonSerializers(
        this IServiceContainer container
    ) => new JsonSerializersConfigurationBuilder(container, Constants.DefaultKey);

    public static IJsonSerializersConfigurationBuilder AddJsonSerializers(
        this IServiceContainer container,
        string key
    ) => new JsonSerializersConfigurationBuilder(container, key);
}