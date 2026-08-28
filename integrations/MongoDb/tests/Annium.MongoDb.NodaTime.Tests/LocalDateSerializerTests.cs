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
    /// Static constructor registering the package's serializers, the same way a consumer does. The
    /// registry is process-wide, so every class here goes through the one entry point rather than
    /// registering its own - two classes registering different instances for one type is a conflict.
    /// Registers the LocalDate serializer
    /// </summary>
    static LocalDateSerializerTests()
    {
        NodaTimeSerializers.Register();
    }

    /// <summary>
    /// Tests that LocalDate values with ISO calendar can be round-tripped correctly
    /// </summary>
    [Fact]
    public void CanRoundTripValueWithIsoCalendar()
    {
        var value = new LocalDate(2015, 1, 1);
        var obj = new Test { LocalDate = value };
        obj.ToTestJson().Contains("'LocalDate' : '2015-01-01'").IsTrue();

        obj = BsonSerializer.Deserialize<Test>(obj.ToBson());
        obj.LocalDate.Is(value, "the round trip must return the value that was written");
    }

    /// <summary>
    /// Tests that LocalDate values are converted to ISO calendar when serializing
    /// </summary>
    [Fact]
    public void ConvertsToIsoCalendarWhenSerializing()
    {
        var value = new LocalDate(2015, 1, 1).WithCalendar(CalendarSystem.PersianSimple);
        var obj = new Test { LocalDate = value };
        obj.ToTestJson().Contains("'LocalDate' : '2015-01-01'").IsTrue();

        obj = BsonSerializer.Deserialize<Test>(obj.ToBson());
        obj.LocalDate.Is(
            value.WithCalendar(CalendarSystem.Iso),
            "the round trip must return what was written, in the ISO calendar"
        );
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
