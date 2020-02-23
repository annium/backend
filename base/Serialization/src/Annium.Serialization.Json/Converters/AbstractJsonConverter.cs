using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Core.Reflection;
using Annium.Extensions.Primitives;

namespace Annium.Serialization.Json.Converters
{
    internal class AbstractJsonConverter<T> : JsonConverter<T>
    {
        public override T Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            var doc = JsonDocument.ParseValue(ref reader);

            var resolutionProperties = typeToConvert.GetProperties()
                .Where(x => x.GetCustomAttribute<ResolveFieldAttribute>() != null)
                .ToList();
            if (resolutionProperties.Count > 1)
                throw new SerializationException(error("type has multiple resolution fields"));
            var resolutionProperty = resolutionProperties.FirstOrDefault();

            Type concreteTypeDefinition;
            if (resolutionProperty is null)
            {
                var properties = doc.RootElement.EnumerateObject()
                    .AsEnumerable()
                    .Select(p => p.Name.ToLowerInvariant())
                    .OrderBy(p => p)
                    .ToArray();

                var definition = TypeManager.Instance.ResolveBySignature(properties, typeToConvert, exact: false);
                if (definition is null)
                    throw new SerializationException(error("no matches by signature"));
                concreteTypeDefinition = definition;
            }
            else
            {
                if (!doc.RootElement.TryGetProperty(resolutionProperty.Name.PascalCase(), out var keyElement) &&
                    !doc.RootElement.TryGetProperty(resolutionProperty.Name.CamelCase(), out keyElement))
                    throw new SerializationException(error("key property is missing"));

                var key = keyElement.GetString();
                var definition = TypeManager.Instance.ResolveByKey(key, typeToConvert);
                if (definition is null)
                    throw new SerializationException($"no match for key {key}");
                concreteTypeDefinition = definition;
            }

            var concreteType = concreteTypeDefinition.ResolveByImplementation(typeToConvert);
            if (concreteType is null)
                throw new SerializationException($"Can't resolve concrete type for {concreteTypeDefinition} by {typeToConvert}");

            return (T)JsonSerializer.Deserialize(doc.RootElement.GetRawText(), concreteType, options);

            string error(string message) => $"Can't resolve concrete type definition for {typeToConvert}: {message}";
        }

        public override void Write(
            Utf8JsonWriter writer,
            T value,
            JsonSerializerOptions options
        )
        {
            JsonSerializer.Serialize(writer, value, value?.GetType() ?? typeof(object), options);
        }
    }
}