using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Extensions.Primitives;

namespace Annium.Data.Operations.Serialization.Json.Internal
{
    internal abstract class ResultConverterBase<T> : JsonConverter<T> where T : IResultBase
    {
        protected void WriteErrors(
            Utf8JsonWriter writer,
            T value,
            JsonSerializerOptions options
        )
        {
            writer.WritePropertyName(nameof(IResultBase.PlainErrors).CamelCase());
            JsonSerializer.Serialize(writer, value.PlainErrors, options);

            writer.WritePropertyName(nameof(IResultBase.LabeledErrors).CamelCase());
            JsonSerializer.Serialize(
                writer,
                options.DictionaryKeyPolicy == JsonNamingPolicy.CamelCase
                    ? value.LabeledErrors.ToDictionary(x => x.Key.CamelCase(), x => x.Value)
                    : value.LabeledErrors,
                options
            );
        }
    }

    internal abstract class ResultConverterBaseFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsInterface
                ? IsConvertibleInterface(objectType)
                : objectType.GetInterfaces().Any(IsConvertibleInterface);
        }

        protected abstract bool IsConvertibleInterface(Type type);

        protected Type GetImplementation(Type type)
        {
            if (type.IsInterface)
                return type;

            return type.GetInterfaces()
                .First(IsConvertibleInterface);
        }
    }
}