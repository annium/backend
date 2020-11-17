using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Core.Primitives;
using X = Annium.Data.Operations.IResult<object>;

namespace Annium.Data.Operations.Serialization.Json.Internal
{
    internal class ResultDataConverter<TD> : ResultConverterBase<IResult<TD>>
    {
        public override IResult<TD> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            TD data = default !;
            IReadOnlyCollection<string> plainErrors = Array.Empty<string>();
            IReadOnlyDictionary<string, IReadOnlyCollection<string>> labeledErrors = new Dictionary<string, IReadOnlyCollection<string>>();

            var depth = reader.CurrentDepth;
            while (reader.Read() && reader.CurrentDepth > depth)
            {
                if (reader.HasProperty(nameof(X.Data)))
                    data = JsonSerializer.Deserialize<TD>(ref reader, options)!;
                else if (reader.HasProperty(nameof(X.PlainErrors)))
                    plainErrors = JsonSerializer.Deserialize<IReadOnlyCollection<string>>(ref reader, options)!;
                else if (reader.HasProperty(nameof(X.LabeledErrors)))
                    labeledErrors = JsonSerializer.Deserialize<IReadOnlyDictionary<string, IReadOnlyCollection<string>>>(ref reader, options)!;
            }

            var value = Result.New(data);

            value.Errors(plainErrors);
            value.Errors(labeledErrors);

            return value;
        }

        public override void Write(
            Utf8JsonWriter writer,
            IResult<TD> value,
            JsonSerializerOptions options
        )
        {
            writer.WriteStartObject();

            writer.WritePropertyName(nameof(X.Data).CamelCase());
            JsonSerializer.Serialize(writer, value.Data, options);

            WriteErrors(writer, value, options);

            writer.WriteEndObject();
        }
    }

    internal class ResultDataConverterFactory : ResultConverterBaseFactory
    {
        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var typeArgs = GetImplementation(typeToConvert).GetGenericArguments();

            return (JsonConverter) Activator.CreateInstance(typeof(ResultDataConverter<>).MakeGenericType(typeArgs[0])) !;
        }

        protected override bool IsConvertibleInterface(Type type) =>
            type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IResult<>);
    }
}