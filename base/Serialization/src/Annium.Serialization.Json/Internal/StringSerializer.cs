using System;
using System.Text.Json;
using Annium.Serialization.Abstractions;

namespace Annium.Serialization.Json.Internal
{
    internal class StringSerializer : ISerializer<string>
    {
        private readonly JsonSerializerOptions _options;

        public StringSerializer(OptionsContainer options)
        {
            _options = options.Value;
        }

        public T Deserialize<T>(string value)
        {
            return JsonSerializer.Deserialize<T>(value, _options)!;
        }

        public object Deserialize(Type type, string value)
        {
            return JsonSerializer.Deserialize(value, type, _options)!;
        }

        public string Serialize<T>(T value)
        {
            return JsonSerializer.Serialize(value, _options);
        }

        public string Serialize(object value)
        {
            return JsonSerializer.Serialize(value, _options);
        }
    }
}