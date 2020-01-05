using System;
using Annium.Serialization.Abstractions;
using MessagePack;

namespace Annium.Serialization.MessagePack
{
    public static class MessagePackSerializer
    {
        private static readonly MessagePackSerializerOptions Opts =
            MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);

        public static ISerializer<ReadOnlyMemory<byte>> Instance { get; } = Serializer.Create<ReadOnlyMemory<byte>>(
            value => global::MessagePack.MessagePackSerializer.Serialize(value, Opts),
            (type, value) => global::MessagePack.MessagePackSerializer.Deserialize(type, value, Opts)
        );
    }
}