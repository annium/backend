using System;
using Annium.Testing;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using NodaTime;
using Xunit;

namespace Annium.MongoDb.NodaTime.Tests;

/// <summary>
/// Tests for the OffsetDateTime serializer functionality
/// </summary>
public class OffsetDateTimeSerializerTests
{
    /// <summary>
    /// Static constructor registering the package's serializers, the same way a consumer does. The
    /// registry is process-wide, so every class here goes through the one entry point rather than
    /// registering its own - two classes registering different instances for one type is a conflict.
    /// Registers the OffsetDateTime serializer
    /// </summary>
    static OffsetDateTimeSerializerTests()
    {
        NodaTimeSerializers.Register();
    }

    /// <summary>
    /// Tests that OffsetDateTime values with ISO calendar can be round-tripped correctly
    /// </summary>
    [Fact]
    public void CanRoundTripValueWithIsoCalendar()
    {
        var value = new LocalDateTime(2015, 1, 2, 3, 4, 5).WithOffset(Offset.FromHours(1));
        var obj = new Test { OffsetDateTime = value };
        obj.ToTestJson().Contains("'OffsetDateTime' : '2015-01-02T03:04:05+01'").IsTrue();

        obj = BsonSerializer.Deserialize<Test>(obj.ToBson());
        obj.OffsetDateTime.Is(value, "the round trip must return the value that was written");
    }

    /// <summary>
    /// Tests that OffsetDateTime values are converted to ISO calendar when serializing
    /// </summary>
    [Fact]
    public void ConvertsToIsoCalendarWhenSerializing()
    {
        var value = new LocalDateTime(2015, 1, 2, 3, 4, 5)
            .WithOffset(Offset.FromHours(1))
            .WithCalendar(CalendarSystem.PersianSimple);
        var obj = new Test { OffsetDateTime = value };
        obj.ToTestJson().Contains("'OffsetDateTime' : '2015-01-02T03:04:05+01'").IsTrue();

        obj = BsonSerializer.Deserialize<Test>(obj.ToBson());
        obj.OffsetDateTime.Is(
            value.WithCalendar(CalendarSystem.Iso),
            "the round trip must return what was written, in the ISO calendar"
        );
    }

    /// <summary>
    /// Tests that deserialization throws FormatException for invalid OffsetDateTime strings and null values
    /// </summary>
    [Fact]
    public void ThrowsWhenDateIsInvalid()
    {
        Wrap.It(() => BsonSerializer.Deserialize<Test>(new BsonDocument(new BsonElement("OffsetDateTime", "bleh"))))
            .Throws<FormatException>();
        Wrap.It(() =>
                BsonSerializer.Deserialize<Test>(new BsonDocument(new BsonElement("OffsetDateTime", BsonNull.Value)))
            )
            .Throws<FormatException>();
    }

    /// <summary>
    /// Tests that nullable OffsetDateTime values can be deserialized from null BSON values
    /// </summary>
    [Fact]
    public void CanParseNullable()
    {
        BsonSerializer
            .Deserialize<Test>(new BsonDocument(new BsonElement("NullableOffsetDateTime", BsonNull.Value)))
            .NullableOffsetDateTime.IsDefault();
    }

    /// <summary>
    /// Test class containing OffsetDateTime properties for serialization testing
    /// </summary>
    private class Test
    {
        /// <summary>
        /// Gets or sets an OffsetDateTime value
        /// </summary>
        public OffsetDateTime OffsetDateTime { get; set; }

        /// <summary>
        /// Gets or sets a nullable OffsetDateTime value
        /// </summary>
        public OffsetDateTime? NullableOffsetDateTime { get; set; }
    }
}
