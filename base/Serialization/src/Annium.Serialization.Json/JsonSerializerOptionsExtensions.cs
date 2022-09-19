using System.Text.Encodings.Web;
using System.Text.Json;
using Annium.Core.Runtime.Types;
using Annium.Serialization.Json.Internal.Converters;
using Annium.Serialization.Json.Internal.Options;

namespace Annium.Core.DependencyInjection;

public static class JsonSerializerOptionsExtensions
{
    public static JsonSerializerOptions ConfigureDefault(
        this JsonSerializerOptions options,
        ITypeManager typeManager
    )
    {
        options.Converters.Insert(0, new EnumJsonConverterFactory());
        options.Converters.Insert(1, new MaterializableJsonConverterFactory());
        options.Converters.Insert(2, new JsonNotIndentedJsonConverterFactory());
        options.Converters.Insert(3, new ObjectArrayJsonConverterFactory());
        options.Converters.Insert(4, new AbstractJsonConverterFactory(typeManager));
        options.Converters.Insert(5, new ConstructorJsonConverterFactory());
        options.Converters.Insert(6, new GenericDictionaryJsonConverterFactory());

        options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        options.UseDefaultNamingPolicy();
        options.IncludeFields = true;

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