using System;
using System.Text.Json;
using Annium.Serialization.Abstractions;

namespace Annium.Serialization.Json
{
    public class ByteArraySerializer : ISerializer<byte[]>
    {
        public static ISerializer<byte[]> Configure(Action<JsonSerializerOptions> configure)
        {
            var options = new JsonSerializerOptions();
            configure(options);

            return new ByteArraySerializer(options);
        }

        private readonly JsonSerializerOptions _options;

        private ByteArraySerializer(JsonSerializerOptions options)
        {
            _options = options;
        }

        public T Deserialize<T>(byte[] value)
        {
            return JsonSerializer.Deserialize<T>(value, _options)!;
        }

        public object Deserialize(Type type, byte[] value)
        {
            return JsonSerializer.Deserialize(value, type, _options)!;
        }

        public byte[] Serialize<T>(T value)
        {
            return JsonSerializer.SerializeToUtf8Bytes(value, _options);
        }

        public byte[] Serialize(object value)
        {
            return JsonSerializer.SerializeToUtf8Bytes(value, _options);
        }
    }
}