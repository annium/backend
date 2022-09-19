using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Serialization.Json.Attributes;

namespace Annium.Serialization.Json.Internal.Converters;

internal class JsonNotIndentedJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.GetCustomAttributes().Any(x => x is JsonNotIndentedAttribute);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return (JsonConverter) Activator.CreateInstance(typeof(JsonNotIndentedJsonConverter<>).MakeGenericType(typeToConvert))!;
    }
}