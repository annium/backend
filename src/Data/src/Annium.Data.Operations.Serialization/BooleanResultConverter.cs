using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Annium.Data.Operations.Serialization
{
    public class BooleanResultConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) =>
            objectType == typeof(BooleanResult) ||
            (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(BooleanResult<>));

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer
        )
        {
            if (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(BooleanResult<>))
                return ReadData(reader, objectType.GetGenericArguments() [0]);

            return ReadBase(reader);
        }

        public override void WriteJson(
            JsonWriter writer,
            object value,
            JsonSerializer serializer
        )
        {
            if (value.GetType().IsGenericType && value.GetType().GetGenericTypeDefinition() == typeof(BooleanResult<>))
                WriteData(writer, value, serializer);
            else
                WriteBase(writer, value, serializer);
        }

        private object ReadBase(JsonReader reader)
        {
            var obj = JObject.Load(reader);

            var result = (obj[nameof(BooleanResult.IsSuccess).CamelCase()]?.Value<bool>() ?? false) ? Result.Success() : Result.Failure();
            if (obj[nameof(BooleanResult.PlainErrors).CamelCase()] is JArray plainErrors)
                result.Errors(plainErrors.ToObject<string[]>());
            if (obj[nameof(BooleanResult.LabeledErrors).CamelCase()] is JObject labeledErrors)
                result.Errors(labeledErrors.ToObject<Dictionary<string, string[]>>().ToDictionary(p => p.Key, p => p.Value as IEnumerable<string>));

            return result;
        }

        private object ReadData(JsonReader reader, Type dataType)
        {
            var obj = JObject.Load(reader);

            var isSuccess = obj[nameof(BooleanResult.IsSuccess).CamelCase()]?.Value<bool>() ?? false;

            var data = obj[nameof(BooleanResult<object>.Data).CamelCase()] == null ?
                (dataType.IsValueType ? Activator.CreateInstance(dataType) : null) :
                obj[nameof(BooleanResult<object>.Data).CamelCase()].ToObject(dataType);

            var result = typeof(Result).GetMethods()
                .First(m => m.Name == (isSuccess ? nameof(Result.Success) : nameof(Result.Failure)) && m.IsGenericMethod)
                .MakeGenericMethod(dataType)
                .Invoke(null, new [] { data });

            if (obj[nameof(BooleanResult.PlainErrors).CamelCase()] is JArray plainErrors)
                typeof(BooleanResult<>).MakeGenericType(dataType)
                .GetMethod(nameof(BooleanResult<object>.Errors), new [] { typeof(ICollection<string>) })
                .Invoke(result, new [] { plainErrors.ToObject<string[]>().ToList() });
            if (obj[nameof(BooleanResult.LabeledErrors).CamelCase()] is JObject labeledErrors)
                typeof(BooleanResult<>).MakeGenericType(dataType)
                .GetMethod(nameof(BooleanResult<object>.Errors), new [] { typeof(IReadOnlyCollection<KeyValuePair<string, IEnumerable<string>>>) })
                .Invoke(result, new [] { labeledErrors.ToObject<Dictionary<string, string[]>>().ToDictionary(p => p.Key, p => p.Value as IEnumerable<string>) });

            return result;
        }

        private void WriteBase(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName(nameof(BooleanResult.IsSuccess).CamelCase());
            serializer.Serialize(writer, value.GetType().GetProperty(nameof(BooleanResult.IsSuccess)).GetGetMethod().Invoke(value, Array.Empty<object>()));

            writer.WritePropertyName(nameof(BooleanResult.IsFailure).CamelCase());
            serializer.Serialize(writer, value.GetType().GetProperty(nameof(BooleanResult.IsFailure)).GetGetMethod().Invoke(value, Array.Empty<object>()));

            writer.WritePropertyName(nameof(BooleanResult.PlainErrors).CamelCase());
            serializer.Serialize(writer, value.GetType().GetProperty(nameof(BooleanResult.PlainErrors)).GetGetMethod().Invoke(value, Array.Empty<object>()));

            writer.WritePropertyName(nameof(BooleanResult.LabeledErrors).CamelCase());
            serializer.Serialize(writer, value.GetType().GetProperty(nameof(BooleanResult.LabeledErrors)).GetGetMethod().Invoke(value, Array.Empty<object>()));

            writer.WriteEndObject();
        }

        private void WriteData(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName(nameof(BooleanResult.IsSuccess).CamelCase());
            serializer.Serialize(writer, value.GetType().GetProperty(nameof(BooleanResult.IsSuccess)).GetGetMethod().Invoke(value, Array.Empty<object>()));

            writer.WritePropertyName(nameof(BooleanResult.IsFailure).CamelCase());
            serializer.Serialize(writer, value.GetType().GetProperty(nameof(BooleanResult.IsFailure)).GetGetMethod().Invoke(value, Array.Empty<object>()));

            writer.WritePropertyName(nameof(BooleanResult<object>.Data).CamelCase());
            serializer.Serialize(writer, value.GetType().GetProperty(nameof(BooleanResult<object>.Data)).GetGetMethod().Invoke(value, Array.Empty<object>()));

            writer.WritePropertyName(nameof(BooleanResult.PlainErrors).CamelCase());
            serializer.Serialize(writer, value.GetType().GetProperty(nameof(BooleanResult.PlainErrors)).GetGetMethod().Invoke(value, Array.Empty<object>()));

            writer.WritePropertyName(nameof(BooleanResult.LabeledErrors).CamelCase());
            serializer.Serialize(writer, value.GetType().GetProperty(nameof(BooleanResult.LabeledErrors)).GetGetMethod().Invoke(value, Array.Empty<object>()));

            writer.WriteEndObject();
        }
    }
}