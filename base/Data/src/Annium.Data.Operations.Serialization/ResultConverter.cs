using System;
using System.Collections.Generic;
using System.Text.Json;
using T = Annium.Data.Operations.IResult;

namespace Annium.Data.Operations.Serialization
{
    public class ResultConverter : ResultConverterBase
    {
        protected override bool IsConvertibleInterface(Type type) => type == typeof(IResult);

        public override IResultBase Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            var value = Result.New();

            while (reader.Read())
            {
                if (reader.HasProperty(nameof(T.PlainErrors)))
                    value.Errors(JsonSerializer.Deserialize<IEnumerable<string>>(ref reader, options));
                if (reader.HasProperty(nameof(T.LabeledErrors)))
                    value.Errors(JsonSerializer.Deserialize<IReadOnlyDictionary<string, IEnumerable<string>>>(ref reader, options));
            }

            return value;
        }

        public override void Write(
            Utf8JsonWriter writer,
            IResultBase value,
            JsonSerializerOptions options
        )
        {
            writer.WriteStartObject();

            WriteErrors(writer, value, options);

            writer.WriteEndObject();
        }
    }
}