using System;
using Annium.Serialization.Abstractions;
using MessagePack;

namespace Annium.Serialization.MessagePack.Internal
{
    internal class MessagePackSerializer : ISerializer<ReadOnlyMemory<byte>>
    {
        private readonly MessagePackSerializerOptions _opts =
            MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);

        public T Deserialize<T>(ReadOnlyMemory<byte> value)
        {
            return global::MessagePack.MessagePackSerializer.Deserialize<T>(value, _opts);
        }

        public object Deserialize(Type type, ReadOnlyMemory<byte> value)
        {
            return global::MessagePack.MessagePackSerializer.Deserialize(type, value, _opts);
        }

        public ReadOnlyMemory<byte> Serialize<T>(T value)
        {
            return global::MessagePack.MessagePackSerializer.Serialize(value, _opts);
        }

        public ReadOnlyMemory<byte> Serialize(object value)
        {
            return global::MessagePack.MessagePackSerializer.Serialize(value, _opts);
        }
    }
}