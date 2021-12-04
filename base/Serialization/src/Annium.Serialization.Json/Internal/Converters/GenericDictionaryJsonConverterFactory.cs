using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Annium.Serialization.Json.Internal.Converters;

internal class GenericDictionaryJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type objectType)
    {
        return ResolveKeyValueTypes(objectType).Any();
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var (key, value) = ResolveKeyValueTypes(typeToConvert).Single();

        return (JsonConverter) Activator.CreateInstance(typeof(GenericDictionaryJsonConverter<,>).MakeGenericType(key, value))!;
    }

    private IEnumerable<(Type, Type)> ResolveKeyValueTypes(Type type) => type
        .GetInterfaces()
        .Select<Type, (Type, Type)?>(x =>
        {
            if (x.IsGenericType &&
                x.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
                x.GenericTypeArguments[0].IsGenericType &&
                x.GenericTypeArguments[0].GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
            {
                var args = x.GenericTypeArguments[0].GenericTypeArguments;

                return (args[0], args[1]);
            }

            return null;
        })
        .OfType<(Type, Type)>();
}