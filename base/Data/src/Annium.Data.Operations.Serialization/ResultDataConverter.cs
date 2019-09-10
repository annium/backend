using System;
using System.Linq;
using Annium.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Annium.Data.Operations.Serialization
{
    public class ResultDataConverter : ResultConverterBase
    {
        protected override bool IsConvertibleInterface(Type type) =>
            type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IResult<>);

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer
        )
        {
            var typeArgs = GetImplementation(objectType).GetGenericArguments();
            var dataType = typeArgs[0];

            var obj = JObject.Load(reader);

            var data = obj.Get(nameof(IResult<object>.Data)) == null ?
                (dataType.IsValueType ? Activator.CreateInstance(dataType) : null) :
                obj.Get(nameof(IResult<object>.Data)).ToObject(dataType, serializer);

            var result = typeof(Result).GetMethods()
                .First(m => m.Name == nameof(Result.New) && m.IsGenericMethod && m.GetGenericArguments().Length == 1)
                .MakeGenericMethod(dataType)
                .Invoke(null, new [] { data });

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

            writer.WritePropertyName(nameof(IResult<object>.Data).CamelCase());
            serializer.Serialize(writer, value.GetPropertyValue(nameof(IResult<object>.Data)));

            WriteErrors(value, writer, serializer);

            writer.WriteEndObject();
        }
    }
}