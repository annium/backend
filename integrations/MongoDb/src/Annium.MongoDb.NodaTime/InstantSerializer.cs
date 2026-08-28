using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using NodaTime;
using NodaTime.Text;

namespace Annium.MongoDb.NodaTime;

/// <summary>
/// BSON serializer for NodaTime Instant values, supporting both DateTime and string representations
/// </summary>
public class InstantSerializer : SerializerBase<Instant>
{
    /// <summary>
    /// Deserializes a BSON value to an Instant, supporting both DateTime and string formats
    /// </summary>
    /// <param name="context">The deserialization context</param>
    /// <param name="args">The deserialization arguments</param>
    /// <returns>The deserialized Instant value</returns>
    public override Instant Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var type = context.Reader.GetCurrentBsonType();
        switch (type)
        {
            case BsonType.DateTime:
                return Instant.FromUnixTimeMilliseconds(context.Reader.ReadDateTime());
            case BsonType.String:
                return InstantPattern.ExtendedIso.CheckedParse(context.Reader.ReadString());
            case BsonType.Null:
                throw new InvalidOperationException("Instant is a value type, but the BsonValue is null.");
            default:
                throw new NotSupportedException($"Cannot convert a {type} to an Instant.");
        }
    }

    /// <summary>
    /// Serializes an Instant to a BSON DateTime value
    /// </summary>
    /// <param name="context">The serialization context</param>
    /// <param name="args">The serialization arguments</param>
    /// <param name="value">The Instant value to serialize</param>
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Instant value)
    {
        // NodaTime's own conversion rather than dividing the tick count: integer division truncates
        // towards zero, which moves an instant before 1970 forwards while one after it moves back, so the
        // same fraction of a millisecond was rounded in opposite directions either side of the epoch
        context.Writer.WriteDateTime(value.ToUnixTimeMilliseconds());
    }
}
