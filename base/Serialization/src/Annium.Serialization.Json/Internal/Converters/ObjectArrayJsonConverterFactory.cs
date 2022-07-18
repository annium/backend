using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Serialization.Json.Attributes;

namespace Annium.Serialization.Json.Internal.Converters;

internal class ObjectArrayJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.GetTypeInfo().GetCustomAttribute<JsonAsArrayAttribute>() != null;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return (JsonConverter) Activator.CreateInstance(typeof(ObjectArrayJsonConverter<>).MakeGenericType(typeToConvert))!;
    }
}