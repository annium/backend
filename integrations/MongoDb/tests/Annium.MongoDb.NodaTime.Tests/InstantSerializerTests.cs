using System;
using Annium.Testing;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using NodaTime;
using Xunit;

namespace Annium.MongoDb.NodaTime.Tests;

/// <summary>
/// Tests for the Instant serializer functionality
/// </summary>
public class InstantSerializerTests
{
    /// <summary>
    /// Static constructor to register the Instant serializer
    /// </summary>
    static InstantSerializerTests()
    {
        BsonSerializer.RegisterSerializer(new InstantSerializer());
    }

    /// <summary>
    /// Tests that valid Instant values can be serialized and deserialized correctly
    /// </summary>
    [Fact]
    public void CanConvertValid()
    {
        var instant = Instant.FromUtc(2015, 1, 1, 0, 0, 1);
        var obj = new Test { Instant = instant };
        obj.ToTestJson().Contains("'Instant' : { '$date' : '2015-01-01T00:00:01Z' }").IsTrue();

        obj = BsonSerializer.Deserialize<Test>(obj.ToBson());
        obj.Instant.Is(instant);
    }

    /// <summary>
    /// Tests that nullable Instant values can be serialized and deserialized correctly
    /// </summary>
    [Fact]
    public void CanConvertNullableValid()
    {
        var instant = Instant.FromUtc(2015, 1, 1, 0, 0, 1);
        var obj = new Test { InstantNullable = instant };
        obj.ToTestJson().Contains("'InstantNullable' : { '$date' : '2015-01-01T00:00:01Z' }").IsTrue();

        obj = BsonSerializer.Deserialize<Test>(obj.ToBson());
        obj.InstantNullable.Is(instant);
    }

    /// <summary>
    /// Tests that nullable Instant properties serialize to null when not set
    /// </summary>
    [Fact]
    public void CanConvertNullableNull()
    {
        var instant = Instant.FromUtc(2015, 1, 1, 0, 0, 1);
        var obj = new Test { Instant = instant };
        obj.ToTestJson().Contains("'Instant' : { '$date' : '2015-01-01T00:00:01Z' }").IsTrue();
        obj.ToTestJson().Contains("'InstantNullable' : null").IsTrue();

        obj = BsonSerializer.Deserialize<Test>(obj.ToBson());
        obj.Instant.Is(instant);
        obj.InstantNullable.IsDefault();
    }

    /// <summary>
    /// Tests that the serializer supports deserializing old string format for Instant values
    /// </summary>
    [Fact]
    public void SupportsOldFormat()
    {
        var instant = Instant.FromUtc(2015, 1, 1, 1, 0, 1);

        var doc = new BsonDocument(new BsonElement("Instant", "2015-01-01T01:00:01Z"));
        var obj = BsonSerializer.Deserialize<Test>(doc);
        obj.Instant.Is(instant);
    }

    /// <summary>
    /// Tests that deserialization throws FormatException for invalid BSON types
    /// </summary>
    [Fact]
    public void ThrowsForInvalidTypes()
    {
        var doc = new BsonDocument(new BsonElement("Instant", new BsonBoolean(false)));
        Wrap.It(() => BsonSerializer.Deserialize<Test>(doc)).Throws<FormatException>();

        var doc2 = new BsonDocument(new BsonElement("Instant", new BsonInt32(1)));
        Wrap.It(() => BsonSerializer.Deserialize<Test>(doc2)).Throws<FormatException>();
    }

    /// <summary>
    /// Tests that deserialization throws FormatException when null is provided for non-nullable Instant
    /// </summary>
    [Fact]
    public void ThrowsForNullWhenNotNullable()
    {
        var doc = new BsonDocument(new BsonElement("Instant", BsonNull.Value));
        Wrap.It(() => BsonSerializer.Deserialize<Test>(doc)).Throws<FormatException>();
    }

    /// <summary>
    /// Tests that nullable Instant values can be deserialized from null BSON values
    /// </summary>
    [Fact]
    public void CanParseNullable()
    {
        BsonSerializer
            .Deserialize<Test>(new BsonDocument(new BsonElement("InstantNullable", BsonNull.Value)))
            .InstantNullable.IsDefault();
    }

    /// <summary>
    /// Test class containing Instant properties for serialization testing
    /// </summary>
    private class Test
    {
        /// <summary>
        /// Gets or sets an Instant value
        /// </summary>
        public Instant Instant { get; set; }

        /// <summary>
        /// Gets or sets a nullable Instant value
        /// </summary>
        public Instant? InstantNullable { get; set; }
    }
}
