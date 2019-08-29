using System;
using Annium.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Annium.Data.Operations.Serialization
{
    public class BooleanResultConverter : ResultConverterBase
    {
        protected override bool IsConvertibleInterface(Type type) =>
            type == typeof(IBooleanResult);

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer
        )
        {
            var obj = JObject.Load(reader);

            var result = (obj.Get(nameof(IBooleanResult.IsSuccess))?.Value<bool>() ?? false) ? Result.Success() : Result.Failure();

            ReadErrors(obj, result);

            return result;
        }

        public override void WriteJson(
            JsonWriter writer,
            object value,
            JsonSerializer serializer
        )
        {
            writer.WriteStartObject();

            writer.WritePropertyName(nameof(IBooleanResult.IsSuccess).CamelCase());
            serializer.Serialize(writer, value.GetPropertyValue(nameof(IBooleanResult.IsSuccess)));

            writer.WritePropertyName(nameof(IBooleanResult.IsFailure).CamelCase());
            serializer.Serialize(writer, value.GetPropertyValue(nameof(IBooleanResult.IsFailure)));

            WriteErrors(value, writer, serializer);

            writer.WriteEndObject();
        }
    }
}