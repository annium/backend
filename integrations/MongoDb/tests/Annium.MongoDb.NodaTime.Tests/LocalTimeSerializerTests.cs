using System;
using Annium.Testing;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using NodaTime;
using Xunit;

namespace Annium.MongoDb.NodaTime.Tests;

/// <summary>
/// Tests for the LocalTime serializer functionality
/// </summary>
public class LocalTimeSerializerTests
{
    /// <summary>
    /// Static constructor to register the LocalTime serializer
    /// </summary>
    static LocalTimeSerializerTests()
    {
        BsonSerializer.RegisterSerializer(new LocalTimeSerializer());
    }

    /// <summary>
    /// Tests that LocalTime values can be round-tripped correctly
    /// </summary>
    [Fact]
    public void CanRoundTripValueWithIsoCalendar()
    {
        var obj = new Test { LocalTime = new LocalTime(13, 25, 1) };
        obj.ToTestJson().Contains("'LocalTime' : '13:25:01'").IsTrue();

        obj = BsonSerializer.Deserialize<Test>(obj.ToBson());
        obj.LocalTime.Is(obj.LocalTime);
    }

    /// <summary>
    /// Tests that deserialization throws FormatException for invalid LocalTime strings and null values
    /// </summary>
    [Fact]
    public void ThrowsWhenDateIsInvalid()
    {
        Wrap.It(() => BsonSerializer.Deserialize<Test>(new BsonDocument(new BsonElement("LocalTime", "bleh"))))
            .Throws<FormatException>();
        Wrap.It(() => BsonSerializer.Deserialize<Test>(new BsonDocument(new BsonElement("LocalTime", BsonNull.Value))))
            .Throws<FormatException>();
    }

    /// <summary>
    /// Tests that nullable LocalTime values can be deserialized from null BSON values
    /// </summary>
    [Fact]
    public void CanParseNullable()
    {
        BsonSerializer
            .Deserialize<Test>(new BsonDocument(new BsonElement("NullableLocalTime", BsonNull.Value)))
            .NullableLocalTime.IsDefault();
    }

    /// <summary>
    /// Test class containing LocalTime properties for serialization testing
    /// </summary>
    private class Test
    {
        /// <summary>
        /// Gets or sets a LocalTime value
        /// </summary>
        public LocalTime LocalTime { get; set; }

        /// <summary>
        /// Gets or sets a nullable LocalTime value
        /// </summary>
        public LocalTime? NullableLocalTime { get; set; }
    }
}
