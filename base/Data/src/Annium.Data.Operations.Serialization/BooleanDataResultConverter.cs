using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Annium.Extensions.Primitives;
using T = Annium.Data.Operations.IBooleanResult<object>;

namespace Annium.Data.Operations.Serialization
{
    public class BooleanDataResultConverter : ResultConverterBase
    {
        protected override bool IsConvertibleInterface(Type type) =>
            type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IBooleanResult<>);

        public override IResultBase Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            var typeArgs = GetImplementation(typeToConvert).GetGenericArguments();
            var dataType = typeArgs[0];

            var isSuccess = false;
            var data = dataType.IsValueType ? Activator.CreateInstance(dataType) : null;
            IEnumerable<string> plainErrors = Array.Empty<string>();
            IReadOnlyDictionary<string, IEnumerable<string>> labeledErrors = new Dictionary<string, IEnumerable<string>>();

            while (reader.Read())
            {
                if (reader.HasProperty(nameof(T.IsSuccess)))
                    isSuccess = JsonSerializer.Deserialize<bool>(ref reader, options);
                if (reader.HasProperty(nameof(T.IsFailure)))
                    isSuccess = !JsonSerializer.Deserialize<bool>(ref reader, options);
                if (reader.HasProperty(nameof(T.Data)))
                    data = JsonSerializer.Deserialize(ref reader, dataType, options);
                if (reader.HasProperty(nameof(T.PlainErrors)))
                    plainErrors = JsonSerializer.Deserialize<IEnumerable<string>>(ref reader, options);
                if (reader.HasProperty(nameof(T.LabeledErrors)))
                    labeledErrors = JsonSerializer.Deserialize<IReadOnlyDictionary<string, IEnumerable<string>>>(ref reader, options);
            }

            var value = (IResultBase) typeof(Result).GetMethods()
                .First(m => m.Name == (isSuccess ? nameof(Result.Success) : nameof(Result.Failure)) && m.IsGenericMethod)
                .MakeGenericMethod(dataType)
                .Invoke(null, new [] { data }) !;

            value.Errors(plainErrors);
            value.Errors(labeledErrors);

            return value;
        }

        public override void Write(
            Utf8JsonWriter writer,
            IResultBase value,
            JsonSerializerOptions options
        )
        {
            writer.WriteStartObject();

            writer.WritePropertyName(nameof(T.IsSuccess).CamelCase());
            JsonSerializer.Serialize(writer, value.Get(nameof(T.IsSuccess)), options);

            writer.WritePropertyName(nameof(T.IsFailure).CamelCase());
            JsonSerializer.Serialize(writer, value.Get(nameof(T.IsFailure)), options);

            writer.WritePropertyName(nameof(T.Data).CamelCase());
            JsonSerializer.Serialize(writer, value.Get(nameof(T.Data)), options);

            WriteErrors(writer, value, options);

            writer.WriteEndObject();
        }
    }
}