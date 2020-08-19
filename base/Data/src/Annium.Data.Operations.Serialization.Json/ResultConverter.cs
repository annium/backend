using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using X = Annium.Data.Operations.IResult;

namespace Annium.Data.Operations.Serialization.Json
{
    internal class ResultConverter : ResultConverterBase<X>
    {
        public override X Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            var value = Result.New();

            var depth = reader.CurrentDepth;
            while (reader.Read() && reader.CurrentDepth > depth)
            {
                if (reader.HasProperty(nameof(X.PlainErrors)))
                    value.Errors(JsonSerializer.Deserialize<IReadOnlyCollection<string>>(ref reader, options));
                else if (reader.HasProperty(nameof(X.LabeledErrors)))
                    value.Errors(JsonSerializer.Deserialize<IReadOnlyDictionary<string, IReadOnlyCollection<string>>>(ref reader, options));
            }

            return value;
        }

        public override void Write(
            Utf8JsonWriter writer,
            X value,
            JsonSerializerOptions options
        )
        {
            writer.WriteStartObject();

            WriteErrors(writer, value, options);

            writer.WriteEndObject();
        }
    }

    internal class ResultConverterFactory : ResultConverterBaseFactory
    {
        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return new ResultConverter();
        }

        protected override bool IsConvertibleInterface(Type type) => type == typeof(X);
    }
}