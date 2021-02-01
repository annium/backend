using System;
using System.Text.Json;
using Annium.Serialization.Abstractions;

namespace Annium.Serialization.Json.Internal
{
    internal class ReadOnlyMemoryByteSerializer : ISerializer<ReadOnlyMemory<byte>>
    {
        private readonly JsonSerializerOptions _options;

        public ReadOnlyMemoryByteSerializer(OptionsContainer options)
        {
            _options = options.Value;
        }

        public T Deserialize<T>(ReadOnlyMemory<byte> value)
        {
            return JsonSerializer.Deserialize<T>(value.Span, _options)!;
        }

        public object Deserialize(Type type, ReadOnlyMemory<byte> value)
        {
            return JsonSerializer.Deserialize(value.Span, type, _options)!;
        }

        public ReadOnlyMemory<byte> Serialize<T>(T value)
        {
            return JsonSerializer.SerializeToUtf8Bytes(value, _options);
        }

        public ReadOnlyMemory<byte> Serialize(object value)
        {
            return JsonSerializer.SerializeToUtf8Bytes(value, _options);
        }
    }
}