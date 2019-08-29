using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Annium.Data.Operations.Serialization
{
    public abstract class ResultConverterBase : JsonConverter
    {
        public override bool CanConvert(Type objectType) =>
            objectType.IsInterface ?
            IsConvertibleInterface(objectType) :
            objectType.GetInterfaces().Any(IsConvertibleInterface);

        protected abstract bool IsConvertibleInterface(Type type);

        protected Type GetImplementation(Type type)
        {
            if (type.IsInterface)
                return type;

            return type.GetInterfaces()
                .First(IsConvertibleInterface);
        }

        protected void ReadErrors(
            JObject obj,
            object result
        )
        {
            if (obj.Get(nameof(IResultBase.PlainErrors)) is JArray plainErrors)
                result.GetType()
                .GetMethod(nameof(IResultBase<object>.Errors), new [] { typeof(ICollection<string>) })
                .Invoke(result, new [] { plainErrors.ToObject<string[]>().ToList() });

            if (obj.Get(nameof(IResultBase.LabeledErrors)) is JObject labeledErrors)
                result.GetType()
                .GetMethod(nameof(IResultBase<object>.Errors), new [] { typeof(IReadOnlyCollection<KeyValuePair<string, IEnumerable<string>>>) })
                .Invoke(result, new [] { labeledErrors.ToObject<Dictionary<string, string[]>>().ToDictionary(p => p.Key, p => p.Value as IEnumerable<string>) });
        }

        protected void WriteErrors(
            object value,
            JsonWriter writer,
            JsonSerializer serializer
        )
        {
            var result = (IResultBase) value;

            writer.WritePropertyName(nameof(IResultBase.PlainErrors).CamelCase());
            serializer.Serialize(writer, result.PlainErrors);

            writer.WritePropertyName(nameof(IResultBase.LabeledErrors).CamelCase());
            serializer.Serialize(writer, result.LabeledErrors);
        }
    }
}