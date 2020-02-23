using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Core.Reflection;

namespace Annium.Serialization.Json.Converters
{
    internal class AbstractJsonConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type objectType)
        {
            // if object type is not interface and object type is not abstract class
            if (!objectType.IsInterface && !(objectType.IsClass && objectType.IsAbstract))
                return false;

            // if implements IEnumerable - likely will be serialized as Json Array, so not suitable for type resolution
            if (objectType.GetInterfaces().Any(
                x => x == typeof(IEnumerable) || (x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            ))
                return false;

            return TypeManager.Instance.CanResolve(
                objectType.IsGenericType ? objectType.GetGenericTypeDefinition() : objectType
            );
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return (JsonConverter) Activator.CreateInstance(typeof(AbstractJsonConverter<>).MakeGenericType(typeToConvert))!;
        }
    }
}