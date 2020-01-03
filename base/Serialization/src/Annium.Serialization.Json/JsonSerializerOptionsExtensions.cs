using System.Text.Encodings.Web;
using System.Text.Json;
using Annium.Serialization.Json;

namespace Annium.Core.DependencyInjection
{
    public static class JsonSerializerOptionsExtensions
    {
        public static JsonSerializerOptions ConfigureAbstractConverter(
            this JsonSerializerOptions options
        )
        {
            options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

            options.Converters.Add(new AbstractJsonConverterFactory());

            return options;
        }
    }
}