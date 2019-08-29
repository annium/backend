using System;
using System.Linq;
using Annium.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Annium.Data.Operations.Serialization
{
    public class StatusResultConverter : ResultConverterBase
    {
        protected override bool IsConvertibleInterface(Type type) =>
            type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IStatusResult<>);

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer
        )
        {
            var statusType = GetImplementation(objectType).GetGenericArguments() [0];

            var obj = JObject.Load(reader);

            var status = obj.Get(nameof(IStatusResult<object>.Status)) == null ?
                (statusType.IsValueType ? Activator.CreateInstance(statusType) : null) :
                obj.Get(nameof(IStatusResult<object>.Status)).ToObject(statusType);

            var result = typeof(Result).GetMethods()
                .First(m => m.Name == nameof(Result.New) && m.IsGenericMethod && m.GetGenericArguments().Length == 1)
                .MakeGenericMethod(statusType)
                .Invoke(null, new [] { status });

            ReadErrors(obj, result);

            return result;
        }

        public override void WriteJson(
            JsonWriter writer,
            object value,
            JsonSerializer serializer
        )
        {
            writer.WriteStartObject();

            writer.WritePropertyName(nameof(IStatusResult<object>.Status).CamelCase());
            serializer.Serialize(writer, value.GetPropertyValue(nameof(IStatusResult<object>.Status)));

            WriteErrors(value, writer, serializer);

            writer.WriteEndObject();
        }
    }
}