using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Annium.Extensions.Primitives;
using T = Annium.Data.Operations.IStatusResult<object, object>;

namespace Annium.Data.Operations.Serialization
{
    public class StatusDataResultConverter : ResultConverterBase
    {
        protected override bool IsConvertibleInterface(Type type) =>
            type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IStatusResult<,>);

        public override IResultBase Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            var typeArgs = GetImplementation(typeToConvert).GetGenericArguments();
            var statusType = typeArgs[0];
            var dataType = typeArgs[1];

            var status = statusType.IsValueType ? Activator.CreateInstance(statusType) : null;
            var data = dataType.IsValueType ? Activator.CreateInstance(dataType) : null;
            IEnumerable<string> plainErrors = Array.Empty<string>();
            IReadOnlyDictionary<string, IEnumerable<string>> labeledErrors = new Dictionary<string, IEnumerable<string>>();

            while (reader.Read())
            {
                if (reader.HasProperty(nameof(T.Status)))
                    status = JsonSerializer.Deserialize(ref reader, statusType, options);
                if (reader.HasProperty(nameof(T.Data)))
                    data = JsonSerializer.Deserialize(ref reader, dataType, options);
                if (reader.HasProperty(nameof(T.PlainErrors)))
                    plainErrors = JsonSerializer.Deserialize<IEnumerable<string>>(ref reader, options);
                if (reader.HasProperty(nameof(T.LabeledErrors)))
                    labeledErrors = JsonSerializer.Deserialize<IReadOnlyDictionary<string, IEnumerable<string>>>(ref reader, options);
            }

            var value = (IResultBase) typeof(Result).GetMethods()
                .First(m => m.Name == nameof(Result.Status) && m.IsGenericMethod && m.GetGenericArguments().Length == 2)
                .MakeGenericMethod(statusType, dataType)
                .Invoke(null, new [] { status, data }) !;

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

            writer.WritePropertyName(nameof(T.Status).CamelCase());
            JsonSerializer.Serialize(writer, value.Get(nameof(T.Status)), options);

            writer.WritePropertyName(nameof(T.Data).CamelCase());
            JsonSerializer.Serialize(writer, value.Get(nameof(T.Data)), options);

            WriteErrors(writer, value, options);

            writer.WriteEndObject();
        }
    }
}