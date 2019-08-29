using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Annium.Data.Operations.Serialization
{
    public class ResultConverter : ResultConverterBase
    {
        protected override bool IsConvertibleInterface(Type type) =>
            type == typeof(IResult);

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer
        )
        {
            var result = Result.New();

            ReadErrors(JObject.Load(reader), result);

            return result;
        }

        public override void WriteJson(
            JsonWriter writer,
            object value,
            JsonSerializer serializer
        )
        {
            writer.WriteStartObject();

            WriteErrors(value, writer, serializer);

            writer.WriteEndObject();
        }
    }
}