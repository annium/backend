using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Annium.Extensions.Primitives;
using T = Annium.Data.Operations.IStatusResult<object>;

namespace Annium.Data.Operations.Serialization
{
    public class StatusResultConverter : ResultConverterBase
    {
        protected override bool IsConvertibleInterface(Type type) =>
            type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IStatusResult<>);

        public override IResultBase Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            var typeArgs = GetImplementation(typeToConvert).GetGenericArguments();
            var statusType = typeArgs[0];

            var status = statusType.IsValueType ? Activator.CreateInstance(statusType) : null;
            IEnumerable<string> plainErrors = Array.Empty<string>();
            IReadOnlyDictionary<string, IEnumerable<string>> labeledErrors = new Dictionary<string, IEnumerable<string>>();

            while (reader.Read())
            {
                if (reader.HasProperty(nameof(T.Status)))
                    status = JsonSerializer.Deserialize(ref reader, statusType, options);
                if (reader.HasProperty(nameof(T.PlainErrors)))
                    plainErrors = JsonSerializer.Deserialize<IEnumerable<string>>(ref reader, options);
                if (reader.HasProperty(nameof(T.LabeledErrors)))
                    labeledErrors = JsonSerializer.Deserialize<IReadOnlyDictionary<string, IEnumerable<string>>>(ref reader, options);
            }

            var value = (IResultBase) typeof(Result).GetMethods()
                .First(m => m.Name == nameof(Result.Status) && m.IsGenericMethod && m.GetGenericArguments().Length == 1)
                .MakeGenericMethod(statusType)
                .Invoke(null, new [] { status }) !;

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

            WriteErrors(writer, value, options);

            writer.WriteEndObject();
        }
    }
}