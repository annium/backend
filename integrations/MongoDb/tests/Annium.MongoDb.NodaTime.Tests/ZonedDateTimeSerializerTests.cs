using System;
using Annium.Testing;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using NodaTime;
using Xunit;

namespace Annium.MongoDb.NodaTime.Tests;

/// <summary>
/// Tests for the ZonedDateTime serializer functionality
/// </summary>
public class ZonedDateTimeSerializerTests
{
    /// <summary>
    /// Eastern timezone used for testing timezone-specific serialization
    /// </summary>
    private static readonly DateTimeZone _easternTimezone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(
        "America/New_York"
    )!;

    /// <summary>
    /// Static constructor to register the ZonedDateTime serializer
    /// </summary>
    static ZonedDateTimeSerializerTests()
    {
        BsonSerializer.RegisterSerializer(new ZonedDateTimeSerializer());
    }

    /// <summary>
    /// Tests that ZonedDateTime values with Eastern timezone can be round-tripped correctly
    /// </summary>
    [Fact]
    public void CanRoundTripValue_Eastern()
    {
        var dateTime = new ZonedDateTime(new LocalDateTime(2015, 1, 2, 3, 4, 5).InUtc().ToInstant(), _easternTimezone);
        var obj = new Test { ZonedDateTime = dateTime };
        obj.ToTestJson().Contains("'ZonedDateTime' : '2015-01-01T22:04:05 America/New_York (-05)'").IsTrue();

        obj = BsonSerializer.Deserialize<Test>(obj.ToBson());
        obj.ZonedDateTime.Is(obj.ZonedDateTime);
        obj.ZonedDateTime.Is(dateTime);
        obj.ZonedDateTime.Zone.Is(_easternTimezone);
    }

    /// <summary>
    /// Tests that ZonedDateTime values with UTC timezone can be round-tripped correctly
    /// </summary>
    [Fact]
    public void CanRoundTripValue_UTC()
    {
        var dateTime = new ZonedDateTime(new LocalDateTime(2015, 1, 2, 3, 4, 5).InUtc().ToInstant(), DateTimeZone.Utc);
        var obj = new Test { ZonedDateTime = dateTime };
        obj.ToTestJson().Contains("'ZonedDateTime' : '2015-01-02T03:04:05 UTC (+00)'").IsTrue();

        obj = BsonSerializer.Deserialize<Test>(obj.ToBson());
        obj.ZonedDateTime.Is(obj.ZonedDateTime);
        obj.ZonedDateTime.Is(dateTime);
        obj.ZonedDateTime.Zone.Is(DateTimeZone.Utc);
    }

    /// <summary>
    /// Tests that deserialization throws FormatException for invalid ZonedDateTime strings and null values
    /// </summary>
    [Fact]
    public void ThrowsWhenDateIsInvalid()
    {
        Wrap.It(() => BsonSerializer.Deserialize<Test>(new BsonDocument(new BsonElement("ZonedDateTime", "bleh"))))
            .Throws<FormatException>();
        Wrap.It(() =>
                BsonSerializer.Deserialize<Test>(new BsonDocument(new BsonElement("ZonedDateTime", BsonNull.Value)))
            )
            .Throws<FormatException>();
    }

    /// <summary>
    /// Tests that nullable ZonedDateTime values can be deserialized from null BSON values
    /// </summary>
    [Fact]
    public void CanParseNullable()
    {
        BsonSerializer
            .Deserialize<Test>(new BsonDocument(new BsonElement("NullableZonedDateTime", BsonNull.Value)))
            .NullableZonedDateTime.IsDefault();
    }

    /// <summary>
    /// Test class containing ZonedDateTime properties for serialization testing
    /// </summary>
    private class Test
    {
        /// <summary>
        /// Gets or sets a ZonedDateTime value
        /// </summary>
        public ZonedDateTime ZonedDateTime { get; set; }

        /// <summary>
        /// Gets or sets a nullable ZonedDateTime value
        /// </summary>
        public ZonedDateTime? NullableZonedDateTime { get; set; }
    }
}
