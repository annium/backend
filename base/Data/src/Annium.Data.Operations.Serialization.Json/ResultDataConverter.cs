using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Extensions.Primitives;
using X = Annium.Data.Operations.IResult<object>;

namespace Annium.Data.Operations.Serialization
{
    internal class ResultDataConverter<D> : ResultConverterBase<IResult<D>>
    {
        public override IResult<D> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            D data = default !;
            IEnumerable<string> plainErrors = Array.Empty<string>();
            IReadOnlyDictionary<string, IEnumerable<string>> labeledErrors = new Dictionary<string, IEnumerable<string>>();

            var depth = reader.CurrentDepth;
            while (reader.Read() && reader.CurrentDepth > depth)
            {
                if (reader.HasProperty(nameof(X.Data)))
                    data = JsonSerializer.Deserialize<D>(ref reader, options);
                else if (reader.HasProperty(nameof(X.PlainErrors)))
                    plainErrors = JsonSerializer.Deserialize<IEnumerable<string>>(ref reader, options);
                else if (reader.HasProperty(nameof(X.LabeledErrors)))
                    labeledErrors = JsonSerializer.Deserialize<IReadOnlyDictionary<string, IEnumerable<string>>>(ref reader, options);
            }

            var value = Result.New(data);

            value.Errors(plainErrors);
            value.Errors(labeledErrors);

            return value;
        }

        public override void Write(
            Utf8JsonWriter writer,
            IResult<D> value,
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