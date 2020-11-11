using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Extensions.Primitives;
using X = Annium.Data.Operations.IBooleanResult;

namespace Annium.Data.Operations.Serialization.Json.Internal
{
    internal class BooleanResultConverter : ResultConverterBase<X>
    {
        public override X Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            var isSuccess = false;
            IReadOnlyCollection<string> plainErrors = Array.Empty<string>();
            IReadOnlyDictionary<string, IReadOnlyCollection<string>> labeledErrors = new Dictionary<string, IReadOnlyCollection<string>>();

            var depth = reader.CurrentDepth;
            while (reader.Read() && reader.CurrentDepth > depth)
            {
                if (reader.HasProperty(nameof(X.IsSuccess)))
                    isSuccess = JsonSerializer.Deserialize<bool>(ref reader, options)!;
                else if (reader.HasProperty(nameof(X.IsFailure)))
                    isSuccess = !JsonSerializer.Deserialize<bool>(ref reader, options);
                else if (reader.HasProperty(nameof(X.PlainErrors)))
                    plainErrors = JsonSerializer.Deserialize<IReadOnlyCollection<string>>(ref reader, options)!;
                else if (reader.HasProperty(nameof(X.LabeledErrors)))
                    labeledErrors = JsonSerializer.Deserialize<IReadOnlyDictionary<string, IReadOnlyCollection<string>>>(ref reader, options)!;
            }

            var value = isSuccess ? Result.Success() : Result.Failure();
            value.Errors(plainErrors);
            value.Errors(labeledErrors);

            return value;
        }

        public override void Write(
            Utf8JsonWriter writer,
            X value,
            JsonSerializerOptions options
        )
        {
            writer.WriteStartObject();

            writer.WritePropertyName(nameof(X.IsSuccess).CamelCase());
            JsonSerializer.Serialize(writer, value.IsSuccess, options);

            writer.WritePropertyName(nameof(X.IsFailure).CamelCase());
            JsonSerializer.Serialize(writer, value.IsFailure, options);

            WriteErrors(writer, value, options);

            writer.WriteEndObject();
        }
    }

    internal class BooleanResultConverterFactory : ResultConverterBaseFactory
    {
        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return new BooleanResultConverter();
        }

        protected override bool IsConvertibleInterface(Type type) => type == typeof(X);
    }
}