using System;
using System.Text.Json;
using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;

namespace Annium.Serialization.Json
{
    public class MemorySerializer : ISerializer<ReadOnlyMemory<byte>>
    {
        static MemorySerializer()
        {
            Default = new MemorySerializer(new JsonSerializerOptions().ConfigureDefault());
        }

        public static ISerializer<ReadOnlyMemory<byte>> Default { get; }

        public static ISerializer<ReadOnlyMemory<byte>> Configure(Action<JsonSerializerOptions> configure)
        {
            var options = new JsonSerializerOptions();
            configure(options);

            return new MemorySerializer(options);
        }

        private readonly JsonSerializerOptions options;

        public MemorySerializer(JsonSerializerOptions options)
        {
            this.options = options;
        }

        public T Deserialize<T>(ReadOnlyMemory<byte> value)
        {
            return JsonSerializer.Deserialize<T>(value.Span, options);
        }

        public object Deserialize(Type type, ReadOnlyMemory<byte> value)
        {
            return JsonSerializer.Deserialize(value.Span, type, options);
        }

        public ReadOnlyMemory<byte> Serialize<T>(T value)
        {
            return JsonSerializer.SerializeToUtf8Bytes(value, options);
        }

        public ReadOnlyMemory<byte> Serialize(object value)
        {
            return JsonSerializer.SerializeToUtf8Bytes(value, options);
        }
    }
}