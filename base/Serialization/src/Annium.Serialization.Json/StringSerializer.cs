using System;
using System.Text.Json;
using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;

namespace Annium.Serialization.Json
{
    public class StringSerializer : ISerializer<string>
    {
        static StringSerializer()
        {
            Default = new StringSerializer(new JsonSerializerOptions().ConfigureDefault());
        }

        public static ISerializer<string> Default { get; }

        public static ISerializer<string> Configure(Action<JsonSerializerOptions> configure)
        {
            var options = new JsonSerializerOptions();
            configure(options);

            return new StringSerializer(options);
        }

        private readonly JsonSerializerOptions options;

        public StringSerializer(JsonSerializerOptions options)
        {
            this.options = options;
        }

        public T Deserialize<T>(string value)
        {
            return JsonSerializer.Deserialize<T>(value, options);
        }

        public object Deserialize(Type type, string value)
        {
            return JsonSerializer.Deserialize(value, type, options);
        }

        public string Serialize<T>(T value)
        {
            return JsonSerializer.Serialize(value, options);
        }

        public string Serialize(object value)
        {
            return JsonSerializer.Serialize(value, options);
        }
    }
}