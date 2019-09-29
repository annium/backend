using System;
using System.Linq;
using Annium.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Annium.Data.Operations.Serialization
{
    public class BooleanDataResultConverter : ResultConverterBase
    {
        protected override bool IsConvertibleInterface(Type type) =>
            type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IBooleanResult<>);

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

            var isSuccess = obj.Get(nameof(IBooleanResult<object>.IsSuccess))?.Value<bool>() ?? false;

            var data = obj.Get(nameof(IBooleanResult<object>.Data)) == null ?
                (dataType.IsValueType ? Activator.CreateInstance(dataType) : null) :
                obj.Get(nameof(IBooleanResult<object>.Data)).ToObject(dataType, serializer);

            var result = typeof(Result).GetMethods()
                .First(m => m.Name == (isSuccess ? nameof(Result.Success) : nameof(Result.Failure)) && m.IsGenericMethod)
                .MakeGenericMethod(dataType)
                .Invoke(null, new [] { data }) !;

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

            writer.WritePropertyName(nameof(IBooleanResult<object>.IsSuccess).CamelCase());
            serializer.Serialize(writer, value.GetPropertyValue(nameof(IBooleanResult<object>.IsSuccess)));

            writer.WritePropertyName(nameof(IBooleanResult<object>.IsFailure).CamelCase());
            serializer.Serialize(writer, value.GetPropertyValue(nameof(IBooleanResult<object>.IsFailure)));

            writer.WritePropertyName(nameof(IBooleanResult<object>.Data).CamelCase());
            serializer.Serialize(writer, value.GetPropertyValue(nameof(IBooleanResult<object>.Data)));

            WriteErrors(value, writer, serializer);

            writer.WriteEndObject();
        }
    }
}