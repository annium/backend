using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Core.Reflection;
using Annium.Core.Runtime.Types;
using Annium.Extensions.Primitives;

namespace Annium.Serialization.Json.Internal.Converters
{
    internal class AbstractJsonConverter<T> : JsonConverter<T>
    {
        private readonly ITypeManager _typeManager;

        public AbstractJsonConverter(
            ITypeManager typeManager
        )
        {
            _typeManager = typeManager;
        }

        public override T Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            var doc = JsonDocument.ParseValue(ref reader);

            var type = ResolveType(doc.RootElement, typeToConvert, options).ResolveByImplementation(typeToConvert);
            if (type is null)
                throw new SerializationException($"Can't resolve concrete type for {type} by {typeToConvert}");

            return (T) JsonSerializer.Deserialize(doc.RootElement.GetRawText(), type, options)!;
        }

        public override void Write(
            Utf8JsonWriter writer,
            T value,
            JsonSerializerOptions options
        )
        {
            JsonSerializer.Serialize(writer, value, value?.GetType() ?? typeof(object), options);
        }

        private Type ResolveType(
            JsonElement root,
            Type baseType,
            JsonSerializerOptions options
        )
        {
            var resolutionKeyProperty = _typeManager.GetResolutionKeyProperty(baseType);

            return resolutionKeyProperty is null
                ? ResolveTypeBySignature(root, baseType)
                : ResolveTypeByKey(root, baseType, resolutionKeyProperty, options);
        }

        private Type ResolveTypeByKey(
            JsonElement root,
            Type baseType,
            PropertyInfo resolutionKeyProperty,
            JsonSerializerOptions options
        )
        {
            if (!root.TryGetProperty(resolutionKeyProperty.Name.PascalCase(), out var keyElement) &&
                !root.TryGetProperty(resolutionKeyProperty.Name.CamelCase(), out keyElement))
                throw new SerializationException(Error(baseType, "key property is missing"));

            var key = JsonSerializer.Deserialize(keyElement.GetRawText(), resolutionKeyProperty.PropertyType, options)!;
            var type = _typeManager.ResolveByKey(key, baseType);
            if (type is null)
                throw new SerializationException(Error(baseType, $"no match for key {key}"));

            return type;
        }

        private Type ResolveTypeBySignature(
            JsonElement root,
            Type baseType
        )
        {
            var properties = root.EnumerateObject().Select(p => p.Name).ToArray();

            var type = _typeManager.ResolveBySignature(properties, baseType);
            if (type is null)
                throw new SerializationException(Error(baseType, "no matches by signature"));

            return type;
        }

        private string Error(Type baseType, string message) =>
            $"Can't resolve concrete type definition for '{baseType}': {message}";
    }
}