using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Extensions.Primitives;

namespace Annium.Data.Operations.Serialization
{
    public abstract class ResultConverterBase : JsonConverter<IResultBase>
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsInterface ?
                IsConvertibleInterface(objectType) :
                objectType.GetInterfaces().Any(IsConvertibleInterface);
        }
        protected abstract bool IsConvertibleInterface(Type type);

        protected Type GetImplementation(Type type)
        {
            if (type.IsInterface)
                return type;

            return type.GetInterfaces()
                .First(IsConvertibleInterface);
        }

        protected void WriteErrors(
            Utf8JsonWriter writer,
            IResultBase value,
            JsonSerializerOptions options
        )
        {
            writer.WritePropertyName(nameof(IResultBase.PlainErrors).CamelCase());
            JsonSerializer.Serialize(writer, value.PlainErrors, options);

            writer.WritePropertyName(nameof(IResultBase.LabeledErrors).CamelCase());
            JsonSerializer.Serialize(writer, value.LabeledErrors, options);
        }
    }
}