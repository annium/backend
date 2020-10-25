using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Annium.Serialization.Json.Internal.Converters
{
    internal class ConstructorJsonConverter<T> : JsonConverter<T>
        where T : notnull
    {
        private readonly ConstructorInfo _constructor;
        private readonly List<ConstructorJsonConverterConfiguration.ParameterItem> _parameters;
        private readonly IReadOnlyCollection<PropertyInfo> _properties;

        public ConstructorJsonConverter(
            ConstructorJsonConverterConfiguration configuration
        )
        {
            (_constructor, _parameters) = configuration;
            _properties = _constructor.DeclaringType!.GetProperties().ToArray();
        }

        public override T Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            var parameters = new object[_parameters.Count];
            var comparison = options.PropertyNameCaseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    var args = parameters
                        .Select((x, i) =>
                        {
                            if (x != null)
                                return x;

                            if (options.IgnoreNullValues)
                                return Activator.CreateInstance(_parameters[i].Type);

                            throw new JsonException();
                        })
                        .ToArray();
                    var result = _constructor.Invoke(args);

                    return (T) result;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException();

                var name = options.PropertyNamingPolicy.ConvertName(reader.GetString());
                var index = _parameters.FindIndex(x => x.Name.Equals(name, comparison));

                // for now - no special handling for extra properties, just skip them
                if (index < 0)
                {
                    var property = _properties.SingleOrDefault(x => x.Name.Equals(name, comparison)) ?? throw new JsonException();
                    JsonSerializer.Deserialize(ref reader, property.PropertyType, options);
                    continue;
                }

                var parameter = _parameters[index];
                var value = JsonSerializer.Deserialize(ref reader, parameter.Type, options);
                parameters[index] = value;
            }

            throw new JsonException();
        }

        public override void Write(
            Utf8JsonWriter writer,
            T value,
            JsonSerializerOptions options
        )
        {
            var opts = options.Clone();
            var factory = opts.Converters.Single(x => x.GetType() == typeof(ConstructorJsonConverterFactory));
            opts.Converters.Remove(factory);

            JsonSerializer.Serialize(writer, value, opts);
        }
    }
}