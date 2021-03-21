using System.Net.Mime;
using Annium.Serialization.Json;
using Annium.Serialization.Json.Internal;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceContainerExtensions
    {
        private const string DefaultKey = MediaTypeNames.Application.Json;

        public static IJsonSerializersConfigurationBuilder AddJsonSerializers(
            this IServiceContainer container
        ) => new JsonSerializersConfigurationBuilder(container, DefaultKey);

        public static IJsonSerializersConfigurationBuilder AddJsonSerializers(
            this IServiceContainer container,
            string key
        ) => new JsonSerializersConfigurationBuilder(container, key);
    }
}