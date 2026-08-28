using System;
using Annium.Testing;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using NodaTime;
using Xunit;

namespace Annium.MongoDb.NodaTime.Tests;

/// <summary>
/// Tests for the LocalDateTime serializer functionality
/// </summary>
public class LocalDateTimeSerializerTests
{
    /// <summary>
    /// Static constructor registering the package's serializers, the same way a consumer does. The
    /// registry is process-wide, so every class here goes through the one entry point rather than
    /// registering its own - two classes registering different instances for one type is a conflict.
    /// Registers the LocalDateTime serializer
    /// </summary>
    static LocalDateTimeSerializerTests()
    {
        NodaTimeSerializers.Register();
    }

    /// <summary>
    /// Tests that LocalDateTime values with ISO calendar can be round-tripped correctly
    /// </summary>
    [Fact]
    public void CanRoundTripValueWithIsoCalendar()
    {
        var value = new LocalDateTime(2015, 1, 2, 3, 4, 5);
        var obj = new Test { LocalDateTime = value };
        obj.ToTestJson().Contains("'LocalDateTime' : '2015-01-02T03:04:05'").IsTrue();

        obj = BsonSerializer.Deserialize<Test>(obj.ToBson());
        obj.LocalDateTime.Is(value, "the round trip must return the value that was written");
    }

    /// <summary>
    /// Tests that LocalDateTime values are converted to ISO calendar when serializing
    /// </summary>
    [Fact]
    public void ConvertsToIsoCalendarWhenSerializing()
    {
        var value = new LocalDateTime(2015, 1, 2, 3, 4, 5).WithCalendar(CalendarSystem.PersianSimple);
        var obj = new Test { LocalDateTime = value };
        obj.ToTestJson().Contains("'LocalDateTime' : '2015-01-02T03:04:05'").IsTrue();

        obj = BsonSerializer.Deserialize<Test>(obj.ToBson());
        obj.LocalDateTime.Is(
            value.WithCalendar(CalendarSystem.Iso),
            "the round trip must return what was written, in the ISO calendar"
        );
    }

    /// <summary>
    /// Tests that deserialization throws FormatException for invalid LocalDateTime strings and null values
    /// </summary>
    [Fact]
    public void ThrowsWhenDateIsInvalid()
    {
        Wrap.It(() => BsonSerializer.Deserialize<Test>(new BsonDocument(new BsonElement("LocalDateTime", "bleh"))))
            .Throws<FormatException>();
        Wrap.It(() =>
                BsonSerializer.Deserialize<Test>(new BsonDocument(new BsonElement("LocalDateTime", BsonNull.Value)))
            )
            .Throws<FormatException>();
    }

    /// <summary>
    /// Tests that nullable LocalDateTime values can be deserialized from null BSON values
    /// </summary>
    [Fact]
    public void CanParseNullable()
    {
        BsonSerializer
            .Deserialize<Test>(new BsonDocument(new BsonElement("NullableLocalDateTime", BsonNull.Value)))
            .NullableLocalDateTime.IsDefault();
    }

    /// <summary>
    /// Test class containing LocalDateTime properties for serialization testing
    /// </summary>
    private class Test
    {
        /// <summary>
        /// Gets or sets a LocalDateTime value
        /// </summary>
        public LocalDateTime LocalDateTime { get; set; }

        /// <summary>
        /// Gets or sets a nullable LocalDateTime value
        /// </summary>
        public LocalDateTime? NullableLocalDateTime { get; set; }
    }
}
