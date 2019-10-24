using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Core.Reflection;

namespace Annium.Data.Serialization.Json
{
    public class AbstractJsonConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type objectType)
        {
            // if object type is interface, or object type is abstract class
            if (!objectType.IsInterface && !(objectType.IsClass && objectType.IsAbstract))
                return false;

            return TypeManager.Instance.CanResolve(
                objectType.IsGenericType ? objectType.GetGenericTypeDefinition() : objectType
            );
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return (JsonConverter)Activator.CreateInstance(typeof(AbstractJsonConverter<>).MakeGenericType(typeToConvert))!;
        }
    }
}