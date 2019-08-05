using Annium.Data.Serialization.Json;
using Newtonsoft.Json;

namespace Annium.Core.DependencyInjection
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