using System;
using Annium.Testing;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using NodaTime;
using Xunit;

namespace Annium.MongoDb.NodaTime.Tests;

/// <summary>
/// Tests for the Duration serializer functionality
/// </summary>
public class DurationSerializerTests
{
    /// <summary>
    /// Static constructor registering the package's serializers, the same way a consumer does. The
    /// registry is process-wide, so every class here goes through the one entry point rather than
    /// registering its own - two classes registering different instances for one type is a conflict.
    /// Registers the Duration serializer
    /// </summary>
    static DurationSerializerTests()
    {
        NodaTimeSerializers.Register();
    }

    /// <summary>
    /// Tests that Duration values can be serialized and deserialized correctly
    /// </summary>
    [Fact]
    public void CanConvertValue()
    {
        var value = Duration.FromSeconds(34);
        var obj = new Test { Duration = value };
        obj.ToTestJson().Contains("'Duration' : '0:00:00:34'").IsTrue();

        obj = BsonSerializer.Deserialize<Test>(obj.ToBson());
        obj.Duration.Is(value, "the round trip must return the value that was written");
    }

    /// <summary>
    /// Tests that deserialization throws FormatException for invalid Duration strings
    /// </summary>
    [Fact]
    public void ThrowsWhenValueIsInvalid()
    {
        Wrap.It(() => BsonSerializer.Deserialize<Test>(new BsonDocument(new BsonElement("Duration", "bleh"))))
            .Throws<FormatException>();
    }

    /// <summary>
    /// Tests that nullable Duration values can be deserialized from null BSON values
    /// </summary>
    [Fact]
    public void CanParseNullable()
    {
        BsonSerializer
            .Deserialize<Test>(new BsonDocument(new BsonElement("DurationNullable", BsonNull.Value)))
            .DurationNullable.IsDefault();
    }

    /// <summary>
    /// A stored value of a type these serializers never write is refused rather than read as something.
    /// The branch lives in the base class every pattern-based serializer here shares, so this covers all
    /// seven of them; only the Instant serializer, which has its own switch, was pinning it before.
    /// </summary>
    [Fact]
    public void ThrowsForUnsupportedBsonType()
    {
        Wrap.It(() => BsonSerializer.Deserialize<Test>(new BsonDocument(new BsonElement("Duration", true))))
            .Throws<FormatException>();
        Wrap.It(() => BsonSerializer.Deserialize<Test>(new BsonDocument(new BsonElement("Duration", 1))))
            .Throws<FormatException>();
    }

    /// <summary>
    /// Test class containing Duration properties for serialization testing
    /// </summary>
    private class Test
    {
        /// <summary>
        /// Gets or sets a Duration value
        /// </summary>
        public Duration Duration { get; set; }

        /// <summary>
        /// Gets or sets a nullable Duration value
        /// </summary>
        public Duration? DurationNullable { get; set; }
    }
}
