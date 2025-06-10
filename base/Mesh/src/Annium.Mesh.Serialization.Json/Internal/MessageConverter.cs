using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Mesh.Domain;

namespace Annium.Mesh.Serialization.Json.Internal;

/// <summary>
/// Custom JSON converter for mesh Message objects that provides compact serialization using short property names.
/// </summary>
internal class MessageConverter : JsonConverter<Message>
{
    /// <summary>
    /// Reads and converts JSON to a Message object.
    /// </summary>
    /// <param name="reader">The Utf8JsonReader to read from.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">Serializer options.</param>
    /// <returns>The deserialized Message object.</returns>
    public override Message? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("deserialization failed");
        }

        var currentDepth = reader.CurrentDepth;

        var id = Guid.Empty;
        ushort version = 0;
        var type = MessageType.None;
        var action = 0;
        ReadOnlyMemory<byte> data = Array.Empty<byte>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject && reader.CurrentDepth == currentDepth)
            {
                if (id == Guid.Empty || type is MessageType.None)
                {
                    return default;
                }

                var message = new Message
                {
                    Id = id,
                    Version = version,
                    Type = type,
                    Action = action,
                    Data = data,
                };

                return message;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();

                reader.Read();

                switch (propertyName)
                {
                    case "i":
                        id = reader.GetGuid();
                        break;
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

    /// <summary>
    /// Writes a Message object to JSON using compact property names.
    /// Property mapping: Id='i', Version='v', Type='t', Action='a', Data='d'.
    /// </summary>
    /// <param name="writer">The Utf8JsonWriter to write to.</param>
    /// <param name="value">The Message value to convert.</param>
    /// <param name="options">Serializer options.</param>
    public override void Write(Utf8JsonWriter writer, Message value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("i", value.Id.ToString());
        writer.WriteNumber("v", value.Version);
        writer.WriteNumber("t", (int)value.Type);
        writer.WriteNumber("a", value.Action);
        writer.WriteBase64String("d", value.Data.Span);
        writer.WriteEndObject();
    }
}
