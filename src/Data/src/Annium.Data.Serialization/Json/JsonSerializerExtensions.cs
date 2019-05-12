using Annium.Data.Serialization.Json;
using Newtonsoft.Json;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ConfigurationExtensions
    {
        public static JsonSerializerSettings ConfigureAbstractConverter(
            this JsonSerializerSettings settings
        )
        {
            settings.Converters.Add(new AbstractJsonConverter());

            return settings;
        }
    }
}