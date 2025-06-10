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
    /// Static constructor to register the Duration serializer
    /// </summary>
    static DurationSerializerTests()
    {
        BsonSerializer.RegisterSerializer(new DurationSerializer());
    }

    /// <summary>
    /// Tests that Duration values can be serialized and deserialized correctly
    /// </summary>
    [Fact]
    public void CanConvertValue()
    {
        var obj = new Test { Duration = Duration.FromSeconds(34) };
        obj.ToTestJson().Contains("'Duration' : '0:00:00:34'").IsTrue();

        obj = BsonSerializer.Deserialize<Test>(obj.ToBson());
        obj.Duration.Is(obj.Duration);
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
