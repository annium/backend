using System;
using System.Buffers;
using Annium.Mesh.Domain;
using MessagePack;
using MessagePack.Formatters;

namespace Annium.Mesh.Serialization.MessagePack.Internal;

internal class MessageFormatter : IMessagePackFormatter<Message>
{
    public Message Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return default!;
        }

        options.Security.DepthStep(ref reader);

        var id = Guid.Empty;
        ushort version = 0;
        var type = MessageType.None;
        var action = 0;
        ReadOnlyMemory<byte> data = Array.Empty<byte>();

        var count = reader.ReadArrayHeader();
        for (var i = 0; i < count; i++)
        {
            switch (i)
            {
                case 0:
                    id = MessagePackSerializer.Deserialize<Guid>(ref reader, options);
                    break;
                case 1:
                    version = reader.ReadUInt16();
                    break;
                case 2:
                    type = (MessageType)reader.ReadByte();
                    break;
                case 3:
                    action = reader.ReadInt32();
                    break;
                case 4:
                    var bytes = reader.ReadBytes();
                    data = bytes.HasValue ? bytes.Value.ToArray() : Array.Empty<byte>();
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        reader.Depth--;

        var message = new Message
        {
            Id = id,
            Version = version,
            Type = type,
            Action = action,
            Data = data
        };

        return message;
    }

    public void Serialize(ref MessagePackWriter writer, Message value, MessagePackSerializerOptions options)
    {
        if (value == null!)
        {
            writer.WriteNil();
            return;
        }

        writer.WriteArrayHeader(5);
        MessagePackSerializer.Serialize(ref writer, value.Id, options);
        writer.Write(value.Version);
        writer.Write((byte)value.Type);
        writer.Write(value.Action);
        writer.Write(value.Data.Span);
    }
}
