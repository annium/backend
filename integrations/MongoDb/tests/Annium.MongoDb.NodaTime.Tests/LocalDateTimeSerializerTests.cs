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
    /// Static constructor to register the LocalDateTime serializer
    /// </summary>
    static LocalDateTimeSerializerTests()
    {
        BsonSerializer.RegisterSerializer(new LocalDateTimeSerializer());
    }

    /// <summary>
    /// Tests that LocalDateTime values with ISO calendar can be round-tripped correctly
    /// </summary>
    [Fact]
    public void CanRoundTripValueWithIsoCalendar()
    {
        var obj = new Test { LocalDateTime = new LocalDateTime(2015, 1, 2, 3, 4, 5) };
        obj.ToTestJson().Contains("'LocalDateTime' : '2015-01-02T03:04:05'").IsTrue();

        obj = BsonSerializer.Deserialize<Test>(obj.ToBson());
        obj.LocalDateTime.Is(obj.LocalDateTime);
    }

    /// <summary>
    /// Tests that LocalDateTime values are converted to ISO calendar when serializing
    /// </summary>
    [Fact]
    public void ConvertsToIsoCalendarWhenSerializing()
    {
        var obj = new Test
        {
            LocalDateTime = new LocalDateTime(2015, 1, 2, 3, 4, 5).WithCalendar(CalendarSystem.PersianSimple),
        };
        obj.ToTestJson().Contains("'LocalDateTime' : '2015-01-02T03:04:05'").IsTrue();

        obj = BsonSerializer.Deserialize<Test>(obj.ToBson());
        obj.LocalDateTime.Is(obj.LocalDateTime.WithCalendar(CalendarSystem.Iso));
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
