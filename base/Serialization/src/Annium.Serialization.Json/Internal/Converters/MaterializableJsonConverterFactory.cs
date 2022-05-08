using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Data.Models;

namespace Annium.Serialization.Json.Internal.Converters;

internal class MaterializableJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.GetInterfaces().Contains(typeof(IMaterializable));
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return (JsonConverter)Activator.CreateInstance(typeof(MaterializableJsonConverter<>).MakeGenericType(typeToConvert))!;
    }
}