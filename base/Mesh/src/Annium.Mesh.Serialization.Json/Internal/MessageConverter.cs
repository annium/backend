using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Mesh.Domain;

namespace Annium.Mesh.Serialization.Json.Internal;

internal class MessageConverter : JsonConverter<Message>
{
    public override Message? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("deserialization failed");
        }

        var currentDepth = reader.CurrentDepth;

        ushort version = 0;
        var type = MessageType.None;
        var action = 0;
        ReadOnlyMemory<byte> data = Array.Empty<byte>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject && reader.CurrentDepth == currentDepth)
            {
                if (type is MessageType.None)
                {
                    return default;
                }

                var message = new Message
                {
                    Version = version,
                    Type = type,
                    Action = action,
                    Data = data
                };

                return message;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();

                reader.Read();

                switch (propertyName)
                {
                    case "v":
                        version = reader.GetUInt16();
                        break;
                    case "t":
                        type = (MessageType)reader.GetInt32();
                        break;
                    case "a":
                        action = reader.GetInt32();
                        break;
                    case "d":
                        data = reader.GetBytesFromBase64();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }
        }

        throw new JsonException("Unexpected end of json");
    }

    public override void Write(Utf8JsonWriter writer, Message value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("v", value.Version);
        writer.WriteNumber("t", (int)value.Type);
        writer.WriteNumber("a", value.Action);
        writer.WriteBase64String("d", value.Data.Span);
        writer.WriteEndObject();
    }
}