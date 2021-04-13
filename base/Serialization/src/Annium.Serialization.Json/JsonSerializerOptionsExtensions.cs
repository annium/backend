using System.Text.Encodings.Web;
using System.Text.Json;
using Annium.Core.Runtime.Types;
using Annium.Serialization.Json.Internal.Converters;
using Annium.Serialization.Json.Internal.Options;

namespace Annium.Core.DependencyInjection
{
    public static class JsonSerializerOptionsExtensions
    {
        public static JsonSerializerOptions ConfigureDefault(
            this JsonSerializerOptions options,
            ITypeManager typeManager
        )
        {
            options.Converters.Add(new EnumJsonConverterFactory());
            options.Converters.Add(new AbstractJsonConverterFactory(typeManager));
            options.Converters.Add(new ConstructorJsonConverterFactory());
            options.Converters.Add(new GenericDictionaryJsonConverterFactory());

            options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            options.UseDefaultNamingPolicy();

            return options;
        }

        public static JsonSerializerOptions UseDefaultNamingPolicy(this JsonSerializerOptions options) =>
            options.UseNamingPolicy(new DefaultJsonNamingPolicy());

        public static JsonSerializerOptions UseCamelCaseNamingPolicy(this JsonSerializerOptions options) =>
            options.UseNamingPolicy(JsonNamingPolicy.CamelCase);

        private static JsonSerializerOptions UseNamingPolicy(this JsonSerializerOptions options, JsonNamingPolicy policy)
        {
            options.DictionaryKeyPolicy = policy;
            options.PropertyNamingPolicy = policy;

            return options;
        }
    }
}