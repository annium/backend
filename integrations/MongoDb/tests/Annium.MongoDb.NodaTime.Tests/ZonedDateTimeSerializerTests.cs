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
    /// Static constructor registering the package's serializers, the same way a consumer does. The
    /// registry is process-wide, so every class here goes through the one entry point rather than
    /// registering its own - two classes registering different instances for one type is a conflict.
    /// Registers the ZonedDateTime serializer
    /// </summary>
    static ZonedDateTimeSerializerTests()
    {
        NodaTimeSerializers.Register();
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
        obj.ZonedDateTime.Is(dateTime);
        obj.ZonedDateTime.Zone.Is(DateTimeZone.Utc);
    }

    /// <summary>
    /// A sub-second ZonedDateTime keeps its fraction. Its siblings - OffsetDateTime, LocalDateTime -
    /// serialize through extended ISO patterns that carry fractional seconds; dropping them here meant a
    /// value came back as a different instant, with nothing to say it had been rounded.
    /// </summary>
    [Fact]
    public void CanRoundTripValue_SubSecond()
    {
        var dateTime = new LocalDateTime(2015, 1, 2, 3, 4, 5).PlusNanoseconds(123456789).InUtc();
        var obj = new Test { ZonedDateTime = dateTime };

        obj = BsonSerializer.Deserialize<Test>(obj.ToBson());

        obj.ZonedDateTime.Is(dateTime, "the fraction of a second must survive the round trip");
    }

    /// <summary>
    /// A value stored before fractional seconds were carried still reads back: the fraction is optional
    /// in the pattern, so documents written by the earlier format parse unchanged.
    /// </summary>
    [Fact]
    public void CanParseValueWithoutFraction()
    {
        var stored = new BsonDocument(new BsonElement("ZonedDateTime", "2015-01-02T03:04:05 UTC (+00)"));

        var obj = BsonSerializer.Deserialize<Test>(stored);

        obj.ZonedDateTime.Is(new LocalDateTime(2015, 1, 2, 3, 4, 5).InUtc());
    }

    /// <summary>
    /// A value in another calendar is written in the ISO calendar, as its three siblings that carry a
    /// calendar already do. The pattern has no calendar-id component, so whatever digits are written are
    /// read back as ISO ones: a Persian date written unchanged comes back as an instant centuries away.
    /// </summary>
    [Fact]
    public void ConvertsToIsoCalendarWhenSerializing()
    {
        // arrange
        var value = new ZonedDateTime(
            Instant.FromUtc(2015, 6, 15, 3, 4, 5),
            DateTimeZone.Utc,
            CalendarSystem.PersianSimple
        );
        var obj = new Test { ZonedDateTime = value };

        // act
        obj = BsonSerializer.Deserialize<Test>(obj.ToBson());

        // assert - the same moment in time, expressed in the ISO calendar
        obj.ZonedDateTime.ToInstant().Is(value.ToInstant(), "the instant must survive the round trip");
        obj.ZonedDateTime.Is(value.WithCalendar(CalendarSystem.Iso));
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
