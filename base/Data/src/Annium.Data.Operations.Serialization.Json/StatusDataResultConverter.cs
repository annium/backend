using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Extensions.Primitives;
using X = Annium.Data.Operations.IStatusResult<object, object>;

namespace Annium.Data.Operations.Serialization.Json
{
    internal class StatusDataResultConverter<S, D> : ResultConverterBase<IStatusResult<S, D>>
    {
        public override IStatusResult<S, D> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            S status = default !;
            D data = default !;
            IReadOnlyCollection<string> plainErrors = Array.Empty<string>();
            IReadOnlyDictionary<string, IReadOnlyCollection<string>> labeledErrors = new Dictionary<string, IReadOnlyCollection<string>>();

            var depth = reader.CurrentDepth;
            while (reader.Read() && reader.CurrentDepth > depth)
            {
                if (reader.HasProperty(nameof(X.Status)))
                    status = JsonSerializer.Deserialize<S>(ref reader, options);
                else if (reader.HasProperty(nameof(X.Data)))
                    data = JsonSerializer.Deserialize<D>(ref reader, options);
                else if (reader.HasProperty(nameof(X.PlainErrors)))
                    plainErrors = JsonSerializer.Deserialize<IReadOnlyCollection<string>>(ref reader, options);
                else if (reader.HasProperty(nameof(X.LabeledErrors)))
                    labeledErrors = JsonSerializer.Deserialize<IReadOnlyDictionary<string, IReadOnlyCollection<string>>>(ref reader, options);
            }

            var value = Result.Status(status, data);

            value.Errors(plainErrors);
            value.Errors(labeledErrors);

            return value;
        }

        public override void Write(
            Utf8JsonWriter writer,
            IStatusResult<S, D> value,
            JsonSerializerOptions options
        )
        {
            writer.WriteStartObject();

            writer.WritePropertyName(nameof(X.Status).CamelCase());
            JsonSerializer.Serialize(writer, value.Status, options);

            writer.WritePropertyName(nameof(X.Data).CamelCase());
            JsonSerializer.Serialize(writer, value.Data, options);

            WriteErrors(writer, value, options);

            writer.WriteEndObject();
        }
    }

    internal class StatusDataResultConverterFactory : ResultConverterBaseFactory
    {
        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var typeArgs = GetImplementation(typeToConvert).GetGenericArguments();

            return (JsonConverter) Activator.CreateInstance(typeof(StatusDataResultConverter<,>).MakeGenericType(typeArgs[0], typeArgs[1])) !;
        }

        protected override bool IsConvertibleInterface(Type type) =>
            type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IStatusResult<,>);
    }
}