using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Serialization.Json.Internal.Options;

namespace Annium.Serialization.Json.Internal.Converters;

internal class JsonNotIndentedJsonConverter<T> : JsonConverter<T>
{
    public override T Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        return JsonSerializer.Deserialize<T>(ref reader, options.Clone().RemoveConverter<JsonNotIndentedJsonConverterFactory>())!;
    }

    public override void Write(
        Utf8JsonWriter writer,
        T value,
        JsonSerializerOptions options
    )
    {
        writer.WriteRawValue(JsonSerializer.Serialize(value, options.Clone().NotIndented().RemoveConverter<JsonNotIndentedJsonConverterFactory>()));
        // JsonSerializer.Serialize(writer, value, options.Clone().NotIndented().RemoveConverter<JsonNotIndentedJsonConverterFactory>());
    }
}