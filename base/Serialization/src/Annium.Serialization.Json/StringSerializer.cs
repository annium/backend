using System;
using System.Text.Json;
using Annium.Serialization.Abstractions;

namespace Annium.Serialization.Json
{
    public class StringSerializer : ISerializer<string>
    {
        public static ISerializer<string> Configure(Action<JsonSerializerOptions> configure)
        {
            var options = new JsonSerializerOptions();
            configure(options);

            return new StringSerializer(options);
        }

        private readonly JsonSerializerOptions _options;

        private StringSerializer(JsonSerializerOptions options)
        {
            this._options = options;
        }

        public T Deserialize<T>(string value)
        {
            return JsonSerializer.Deserialize<T>(value, _options);
        }

        public object Deserialize(Type type, string value)
        {
            return JsonSerializer.Deserialize(value, type, _options);
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