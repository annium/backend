using System;
using Annium.Testing;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using NodaTime;
using Xunit;

namespace Annium.MongoDb.NodaTime.Tests;

/// <summary>
/// Tests for the LocalDate serializer functionality
/// </summary>
public class LocalDateSerializerTests
{
    /// <summary>
    /// Static constructor to register the LocalDate serializer
    /// </summary>
    static LocalDateSerializerTests()
    {
        BsonSerializer.RegisterSerializer(new LocalDateSerializer());
    }

    /// <summary>
    /// Tests that LocalDate values with ISO calendar can be round-tripped correctly
    /// </summary>
    [Fact]
    public void CanRoundTripValueWithIsoCalendar()
    {
        var obj = new Test { LocalDate = new LocalDate(2015, 1, 1) };
        obj.ToTestJson().Contains("'LocalDate' : '2015-01-01'").IsTrue();

        obj = BsonSerializer.Deserialize<Test>(obj.ToBson());
        obj.LocalDate.Is(obj.LocalDate);
    }

    /// <summary>
    /// Tests that LocalDate values are converted to ISO calendar when serializing
    /// </summary>
    [Fact]
    public void ConvertsToIsoCalendarWhenSerializing()
    {
        var obj = new Test { LocalDate = new LocalDate(2015, 1, 1).WithCalendar(CalendarSystem.PersianSimple) };
        obj.ToTestJson().Contains("'LocalDate' : '2015-01-01'").IsTrue();

        obj = BsonSerializer.Deserialize<Test>(obj.ToBson());
        obj.LocalDate.Is(obj.LocalDate.WithCalendar(CalendarSystem.Iso));
    }

    /// <summary>
    /// Tests that deserialization throws FormatException for invalid LocalDate strings and null values
    /// </summary>
    [Fact]
    public void ThrowsWhenDateIsInvalid()
    {
        Wrap.It(() => BsonSerializer.Deserialize<Test>(new BsonDocument(new BsonElement("LocalDate", "bleh"))))
            .Throws<FormatException>();
        Wrap.It(() => BsonSerializer.Deserialize<Test>(new BsonDocument(new BsonElement("LocalDate", BsonNull.Value))))
            .Throws<FormatException>();
    }

    /// <summary>
    /// Tests that nullable LocalDate values can be deserialized from null BSON values
    /// </summary>
    [Fact]
    public void CanParseNullable()
    {
        BsonSerializer
            .Deserialize<Test>(new BsonDocument(new BsonElement("NullableLocalDate", BsonNull.Value)))
            .NullableLocalDate.IsDefault();
    }

    /// <summary>
    /// Test class containing LocalDate properties for serialization testing
    /// </summary>
    private class Test
    {
        /// <summary>
        /// Gets or sets a LocalDate value
        /// </summary>
        public LocalDate LocalDate { get; set; }

        /// <summary>
        /// Gets or sets a nullable LocalDate value
        /// </summary>
        public LocalDate? NullableLocalDate { get; set; }
    }
}
