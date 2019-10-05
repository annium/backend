using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Core.Reflection;

namespace Annium.Data.Serialization.Json
{
    public class AbstractJsonConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type objectType) => TypeManager.Instance.CanResolve(objectType);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return (JsonConverter) Activator.CreateInstance(typeof(AbstractJsonConverter<>).MakeGenericType(typeToConvert)) !;
        }
    }
}