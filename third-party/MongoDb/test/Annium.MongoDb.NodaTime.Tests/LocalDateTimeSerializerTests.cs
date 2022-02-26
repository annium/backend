using System;
using Annium.Testing;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using NodaTime;
using Xunit;

namespace Annium.MongoDb.NodaTime.Tests;

public class LocalDateTimeSerializerTests
{
    static LocalDateTimeSerializerTests()
    {
        BsonSerializer.RegisterSerializer(new LocalDateTimeSerializer());
    }

    [Fact]
    public void CanRoundTripValueWithIsoCalendar()
    {
        var obj = new Test { LocalDateTime = new LocalDateTime(2015, 1, 2, 3, 4, 5) };
        obj.ToTestJson().Contains("'LocalDateTime' : '2015-01-02T03:04:05'").IsTrue();

        obj = BsonSerializer.Deserialize<Test>(obj.ToBson());
        obj.LocalDateTime.IsEqual(obj.LocalDateTime);
    }

    [Fact]
    public void ConvertsToIsoCalendarWhenSerializing()
    {
        var obj = new Test { LocalDateTime = new LocalDateTime(2015, 1, 2, 3, 4, 5).WithCalendar(CalendarSystem.PersianSimple) };
        obj.ToTestJson().Contains("'LocalDateTime' : '2015-01-02T03:04:05'").IsTrue();

        obj = BsonSerializer.Deserialize<Test>(obj.ToBson());
        obj.LocalDateTime.IsEqual(obj.LocalDateTime.WithCalendar(CalendarSystem.Iso));
    }

    [Fact]
    public void ThrowsWhenDateIsInvalid()
    {
        Wrap.It(() => BsonSerializer.Deserialize<Test>(new BsonDocument(new BsonElement("LocalDateTime", "bleh")))).Throws<FormatException>();
        Wrap.It(() => BsonSerializer.Deserialize<Test>(new BsonDocument(new BsonElement("LocalDateTime", BsonNull.Value)))).Throws<FormatException>();
    }

    [Fact]
    public void CanParseNullable()
    {
        BsonSerializer.Deserialize<Test>(new BsonDocument(new BsonElement("NullableLocalDateTime", BsonNull.Value))).NullableLocalDateTime.IsDefault();
    }

    private class Test
    {
        public LocalDateTime LocalDateTime { get; set; }

        public LocalDateTime? NullableLocalDateTime { get; set; }
    }
}