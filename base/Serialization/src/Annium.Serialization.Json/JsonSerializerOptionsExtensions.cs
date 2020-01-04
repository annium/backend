using System.Text.Encodings.Web;
using System.Text.Json;
using Annium.Serialization.Json;

namespace Annium.Core.DependencyInjection
{
    public static class JsonSerializerOptionsExtensions
    {
        public static JsonSerializerOptions ConfigureDefault(
            this JsonSerializerOptions options
        )
        {
            options.Converters.Add(new AbstractJsonConverterFactory());

            options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            options.PropertyNameCaseInsensitive = true;
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

            return options;
        }
    }
}