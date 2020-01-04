using Annium.Serialization.Abstractions;
using MessagePack;

namespace Annium.Serialization.MessagePack
{
    public static class MessagePackSerializer
    {
        private static readonly MessagePackSerializerOptions Opts =
            MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);

        public static ISerializer<byte[]> Instance { get; } = Serializer.Create(
            value => global::MessagePack.MessagePackSerializer.Serialize(value, Opts),
            (type, value) => global::MessagePack.MessagePackSerializer.Deserialize(type, value, Opts)
        );
    }
}