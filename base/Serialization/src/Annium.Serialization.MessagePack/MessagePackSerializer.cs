using System;
using Annium.Serialization.Abstractions;
using MessagePack;

namespace Annium.Serialization.MessagePack
{
    public class MessagePackSerializer : ISerializer<ReadOnlyMemory<byte>>
    {
        public static ISerializer<ReadOnlyMemory<byte>> Instance { get; } = new MessagePackSerializer();

        private readonly MessagePackSerializerOptions opts =
            MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);

        private MessagePackSerializer()
        {
        }

        public T Deserialize<T>(ReadOnlyMemory<byte> value)
        {
            return global::MessagePack.MessagePackSerializer.Deserialize<T>(value, opts);
        }

        public object Deserialize(Type type, ReadOnlyMemory<byte> value)
        {
            return global::MessagePack.MessagePackSerializer.Deserialize(type, value, opts);
        }

        public ReadOnlyMemory<byte> Serialize<T>(T value)
        {
            return global::MessagePack.MessagePackSerializer.Serialize(value, opts);
        }

        public ReadOnlyMemory<byte> Serialize(object value)
        {
            return global::MessagePack.MessagePackSerializer.Serialize(value, opts);
        }
    }
}