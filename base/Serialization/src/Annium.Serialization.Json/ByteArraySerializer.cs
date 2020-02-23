using System;
using System.Text.Json;
using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;

namespace Annium.Serialization.Json
{
    public class ByteArraySerializer : ISerializer<byte[]>
    {
        static ByteArraySerializer()
        {
            Default = new ByteArraySerializer(new JsonSerializerOptions().ConfigureDefault());
        }

        public static ISerializer<byte[]> Default { get; }

        public static ISerializer<byte[]> Configure(Action<JsonSerializerOptions> configure)
        {
            var options = new JsonSerializerOptions();
            configure(options);

            return new ByteArraySerializer(options);
        }

        private readonly JsonSerializerOptions options;

        private ByteArraySerializer(JsonSerializerOptions options)
        {
            this.options = options;
        }

        public T Deserialize<T>(byte[] value)
        {
            return JsonSerializer.Deserialize<T>(value, options);
        }

        public object Deserialize(Type type, byte[] value)
        {
            return JsonSerializer.Deserialize(value, type, options);
        }

        public byte[] Serialize<T>(T value)
        {
            return JsonSerializer.SerializeToUtf8Bytes(value, options);
        }

        public byte[] Serialize(object value)
        {
            return JsonSerializer.SerializeToUtf8Bytes(value, options);
        }
    }
}