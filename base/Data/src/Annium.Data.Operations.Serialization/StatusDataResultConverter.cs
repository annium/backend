using System;
using System.Linq;
using Annium.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Annium.Data.Operations.Serialization
{
    public class StatusDataResultConverter : ResultConverterBase
    {
        protected override bool IsConvertibleInterface(Type type) =>
            type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IStatusResult<,>);

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer
        )
        {
            var typeArgs = GetImplementation(objectType).GetGenericArguments();
            var statusType = typeArgs[0];
            var dataType = typeArgs[1];

            var obj = JObject.Load(reader);

            var status = obj.Get(nameof(IStatusResult<object, object>.Status)) == null ?
                (statusType.IsValueType ? Activator.CreateInstance(statusType) : null) :
                obj.Get(nameof(IStatusResult<object, object>.Status)).ToObject(statusType, serializer);

            var data = obj.Get(nameof(IStatusResult<object, object>.Data)) == null ?
                (dataType.IsValueType ? Activator.CreateInstance(dataType) : null) :
                obj.Get(nameof(IStatusResult<object, object>.Data)).ToObject(dataType, serializer);

            var result = typeof(Result).GetMethods()
                .First(m => m.Name == nameof(Result.Status) && m.IsGenericMethod && m.GetGenericArguments().Length == 2)
                .MakeGenericMethod(statusType, dataType)
                .Invoke(null, new [] { status, data })!;

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

            writer.WritePropertyName(nameof(IStatusResult<object, object>.Status).CamelCase());
            serializer.Serialize(writer, value.GetPropertyValue(nameof(IStatusResult<object, object>.Status)));

            writer.WritePropertyName(nameof(IStatusResult<object, object>.Data).CamelCase());
            serializer.Serialize(writer, value.GetPropertyValue(nameof(IStatusResult<object, object>.Data)));

            WriteErrors(value, writer, serializer);

            writer.WriteEndObject();
        }
    }
}