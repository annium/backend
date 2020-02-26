using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Extensions.Primitives;
using X = Annium.Data.Operations.IBooleanResult<object>;

namespace Annium.Data.Operations.Serialization
{
    internal class BooleanDataResultConverter<D> : ResultConverterBase<IBooleanResult<D>>
    {
        public override IBooleanResult<D> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            var isSuccess = false;
            D data = default !;
            IEnumerable<string> plainErrors = Array.Empty<string>();
            IReadOnlyDictionary<string, IEnumerable<string>> labeledErrors = new Dictionary<string, IEnumerable<string>>();

            var depth = reader.CurrentDepth;
            while (reader.Read() && reader.CurrentDepth > depth)
            {
                if (reader.HasProperty(nameof(X.IsSuccess)))
                    isSuccess = JsonSerializer.Deserialize<bool>(ref reader, options);
                else if (reader.HasProperty(nameof(X.IsFailure)))
                    isSuccess = !JsonSerializer.Deserialize<bool>(ref reader, options);
                else if (reader.HasProperty(nameof(X.Data)))
                    data = JsonSerializer.Deserialize<D>(ref reader, options);
                else if (reader.HasProperty(nameof(X.PlainErrors)))
                    plainErrors = JsonSerializer.Deserialize<IEnumerable<string>>(ref reader, options);
                else if (reader.HasProperty(nameof(X.LabeledErrors)))
                    labeledErrors = JsonSerializer.Deserialize<IReadOnlyDictionary<string, IEnumerable<string>>>(ref reader, options);
            }

            var value = isSuccess ? Result.Success(data) : Result.Failure(data);

            value.Errors(plainErrors);
            value.Errors(labeledErrors);

            return value;
        }

        public override void Write(
            Utf8JsonWriter writer,
            IBooleanResult<D> value,
            JsonSerializerOptions options
        )
        {
            writer.WriteStartObject();

            writer.WritePropertyName(nameof(X.IsSuccess).CamelCase());
            JsonSerializer.Serialize(writer, value.IsSuccess, options);

            writer.WritePropertyName(nameof(X.IsFailure).CamelCase());
            JsonSerializer.Serialize(writer, value.IsFailure, options);

            writer.WritePropertyName(nameof(X.Data).CamelCase());
            JsonSerializer.Serialize(writer, value.Data, options);

            WriteErrors(writer, value, options);

            writer.WriteEndObject();
        }
    }

    internal class BooleanDataResultConverterFactory : ResultConverterBaseFactory
    {
        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var typeArgs = GetImplementation(typeToConvert).GetGenericArguments();

            return (JsonConverter) Activator.CreateInstance(typeof(BooleanDataResultConverter<>).MakeGenericType(typeArgs[0])) !;
        }

        protected override bool IsConvertibleInterface(Type type) =>
            type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IBooleanResult<>);
    }
}