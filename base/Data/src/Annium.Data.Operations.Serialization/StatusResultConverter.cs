using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Annium.Data.Operations.Serialization
{
    public class StatusResultConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) =>
            objectType.IsGenericType &&
            (objectType.GetGenericTypeDefinition() == typeof(StatusResult<>) ||
                objectType.GetGenericTypeDefinition() == typeof(StatusResult<,>));

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer
        )
        {
            if (objectType.GetGenericTypeDefinition() == typeof(StatusResult<,>))
                return ReadData(reader, objectType.GetGenericArguments() [0], objectType.GetGenericArguments() [1]);

            return ReadBase(reader, objectType.GetGenericArguments() [0]);
        }

        public override void WriteJson(
            JsonWriter writer,
            object value,
            JsonSerializer serializer
        )
        {
            if (value.GetType().GetGenericTypeDefinition() == typeof(StatusResult<,>))
                WriteData(writer, value, serializer);
            else
                WriteBase(writer, value, serializer);
        }

        private object ReadBase(JsonReader reader, Type statusType)
        {
            var obj = JObject.Load(reader);

            var status = obj.Get(nameof(StatusResult<object>.Status)) == null ?
                (statusType.IsValueType ? Activator.CreateInstance(statusType) : null) :
                obj.Get(nameof(StatusResult<object>.Status)).ToObject(statusType);

            var result = typeof(Result).GetMethods()
                .First(m => m.Name == nameof(Result.New) && m.IsGenericMethod && m.GetGenericArguments().Length == 1)
                .MakeGenericMethod(statusType)
                .Invoke(null, new [] { status });
            if (obj.Get(nameof(StatusResult<object>.PlainErrors)) is JArray plainErrors)
                typeof(StatusResult<>).MakeGenericType(statusType)
                .GetMethod(nameof(StatusResult<object>.Errors), new [] { typeof(ICollection<string>) })
                .Invoke(result, new [] { plainErrors.ToObject<string[]>().ToList() });
            if (obj.Get(nameof(StatusResult<object>.LabeledErrors)) is JObject labeledErrors)
                typeof(StatusResult<>).MakeGenericType(statusType)
                .GetMethod(nameof(StatusResult<object>.Errors), new [] { typeof(IReadOnlyCollection<KeyValuePair<string, IEnumerable<string>>>) })
                .Invoke(result, new [] { labeledErrors.ToObject<Dictionary<string, string[]>>().ToDictionary(p => p.Key, p => p.Value as IEnumerable<string>) });

            return result;
        }

        private object ReadData(JsonReader reader, Type statusType, Type dataType)
        {
            var obj = JObject.Load(reader);

            var status = obj.Get(nameof(StatusResult<object>.Status)) == null ?
                (statusType.IsValueType ? Activator.CreateInstance(statusType) : null) :
                obj.Get(nameof(StatusResult<object>.Status)).ToObject(statusType);

            var data = obj.Get(nameof(StatusResult<object, object>.Data)) == null ?
                (dataType.IsValueType ? Activator.CreateInstance(dataType) : null) :
                obj.Get(nameof(StatusResult<object, object>.Data)).ToObject(dataType);

            var result = typeof(Result).GetMethods()
                .First(m => m.Name == nameof(Result.New) && m.IsGenericMethod && m.GetGenericArguments().Length == 2)
                .MakeGenericMethod(statusType, dataType)
                .Invoke(null, new [] { status, data });
            if (obj.Get(nameof(StatusResult<object>.PlainErrors)) is JArray plainErrors)
                typeof(StatusResult<,>).MakeGenericType(statusType, dataType)
                .GetMethod(nameof(StatusResult<object>.Errors), new [] { typeof(ICollection<string>) })
                .Invoke(result, new [] { plainErrors.ToObject<string[]>().ToList() });
            if (obj.Get(nameof(StatusResult<object>.LabeledErrors)) is JObject labeledErrors)
                typeof(StatusResult<,>).MakeGenericType(statusType, dataType)
                .GetMethod(nameof(StatusResult<object>.Errors), new [] { typeof(IReadOnlyCollection<KeyValuePair<string, IEnumerable<string>>>) })
                .Invoke(result, new [] { labeledErrors.ToObject<Dictionary<string, string[]>>().ToDictionary(p => p.Key, p => p.Value as IEnumerable<string>) });

            return result;
        }

        private void WriteBase(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName(nameof(StatusResult<object>.Status).CamelCase());
            serializer.Serialize(writer, value.GetType().GetProperty(nameof(StatusResult<object>.Status)).GetGetMethod().Invoke(value, Array.Empty<object>()));

            writer.WritePropertyName(nameof(StatusResult<object>.PlainErrors).CamelCase());
            serializer.Serialize(writer, value.GetType().GetProperty(nameof(StatusResult<object>.PlainErrors)).GetGetMethod().Invoke(value, Array.Empty<object>()));

            writer.WritePropertyName(nameof(StatusResult<object>.LabeledErrors).CamelCase());
            serializer.Serialize(writer, value.GetType().GetProperty(nameof(StatusResult<object>.LabeledErrors)).GetGetMethod().Invoke(value, Array.Empty<object>()));

            writer.WriteEndObject();
        }

        private void WriteData(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName(nameof(StatusResult<object>.Status).CamelCase());
            serializer.Serialize(writer, value.GetType().GetProperty(nameof(StatusResult<object>.Status)).GetGetMethod().Invoke(value, Array.Empty<object>()));

            writer.WritePropertyName(nameof(StatusResult<object, object>.Data).CamelCase());
            serializer.Serialize(writer, value.GetType().GetProperty(nameof(StatusResult<object, object>.Data)).GetGetMethod().Invoke(value, Array.Empty<object>()));

            writer.WritePropertyName(nameof(StatusResult<object>.PlainErrors).CamelCase());
            serializer.Serialize(writer, value.GetType().GetProperty(nameof(StatusResult<object>.PlainErrors)).GetGetMethod().Invoke(value, Array.Empty<object>()));

            writer.WritePropertyName(nameof(StatusResult<object>.LabeledErrors).CamelCase());
            serializer.Serialize(writer, value.GetType().GetProperty(nameof(StatusResult<object>.LabeledErrors)).GetGetMethod().Invoke(value, Array.Empty<object>()));

            writer.WriteEndObject();
        }
    }
}