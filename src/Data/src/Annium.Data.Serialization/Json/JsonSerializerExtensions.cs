using System.Linq;
using Annium.Data.Serialization.Json;
using Newtonsoft.Json;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ConfigurationExtensions
    {
        public static JsonSerializerSettings ConfigureAbstractConverter(
            this JsonSerializerSettings settings,
            params string[] abstractTypesSources
        )
        {
            abstractTypesSources = abstractTypesSources.OfType<string>().ToArray();
            settings.Converters.Add(new AbstractJsonConverter(abstractTypesSources));

            return settings;
        }
    }
}