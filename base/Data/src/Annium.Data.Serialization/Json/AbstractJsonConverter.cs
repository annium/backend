using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Core.Reflection;

namespace Annium.Data.Serialization.Json
{
    public class AbstractJsonConverter<T> : JsonConverter<T>
    {
        public override T Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            var doc = JsonDocument.ParseValue(ref reader);
            var properties = doc.RootElement.EnumerateObject()
                .AsEnumerable()
                .Select(p => p.Name.ToLowerInvariant())
                .OrderBy(p => p)
                .ToArray();

            var type = TypeManager.Instance.ResolveBySignature(properties, typeToConvert, exact : true);

            return (T) JsonSerializer.Deserialize(doc.RootElement.GetRawText(), type, options);
        }

        public override void Write(
            Utf8JsonWriter writer,
            T value,
            JsonSerializerOptions options
        )
        {
            JsonSerializer.Serialize(writer, value, value?.GetType() ?? typeof(object), options);
        }
    }
}