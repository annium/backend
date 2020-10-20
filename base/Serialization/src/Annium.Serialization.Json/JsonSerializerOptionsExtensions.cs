using System.Text.Encodings.Web;
using System.Text.Json;
using Annium.Core.Runtime.Types;
using Annium.Serialization.Json.Converters;

namespace Annium.Core.DependencyInjection
{
    public static class JsonSerializerOptionsExtensions
    {
        public static JsonSerializerOptions ConfigureDefault(
            this JsonSerializerOptions options,
            ITypeManager typeManager,
            bool useCamelCase = true
        )
        {
            options.Converters.Add(new AbstractJsonConverterFactory(typeManager));
            options.Converters.Add(new GenericDictionaryJsonConverterFactory());


            options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            options.PropertyNameCaseInsensitive = true;

            if (useCamelCase)
            {
                options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            }

            return options;
        }
    }
}