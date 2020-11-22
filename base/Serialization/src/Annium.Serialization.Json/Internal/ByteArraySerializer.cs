using System;
using System.Text.Json;
using Annium.Serialization.Abstractions;

namespace Annium.Serialization.Json.Internal
{
    internal class ByteArraySerializer : ISerializer<byte[]>
    {
        private readonly JsonSerializerOptions _options;

        public ByteArraySerializer(OptionsContainer options)
        {
            _options = options.Value;
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