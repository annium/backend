using System;
using Annium.Testing;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using NodaTime;
using Xunit;

namespace Annium.MongoDb.NodaTime.Tests;

public class OffsetDateTimeSerializerTests
{
    static OffsetDateTimeSerializerTests()
    {
        BsonSerializer.RegisterSerializer(new OffsetDateTimeSerializer());
    }

    [Fact]
    public void CanRoundTripValueWithIsoCalendar()
    {
        var obj = new Test { OffsetDateTime = new LocalDateTime(2015, 1, 2, 3, 4, 5).WithOffset(Offset.FromHours(1)) };
        obj.ToTestJson().Contains("'OffsetDateTime' : '2015-01-02T03:04:05+01'").IsTrue();

        obj = BsonSerializer.Deserialize<Test>(obj.ToBson());
        obj.OffsetDateTime.Is(obj.OffsetDateTime);
    }

    [Fact]
    public void ConvertsToIsoCalendarWhenSerializing()
    {
        var obj = new Test
        {
            OffsetDateTime = new LocalDateTime(2015, 1, 2, 3, 4, 5)
                .WithOffset(Offset.FromHours(1))
                .WithCalendar(CalendarSystem.PersianSimple)
        };
        obj.ToTestJson().Contains("'OffsetDateTime' : '2015-01-02T03:04:05+01'").IsTrue();

        obj = BsonSerializer.Deserialize<Test>(obj.ToBson());
        obj.OffsetDateTime.Is(obj.OffsetDateTime.WithCalendar(CalendarSystem.Iso));
    }

    [Fact]
    public void ThrowsWhenDateIsInvalid()
    {
        Wrap.It(() => BsonSerializer.Deserialize<Test>(new BsonDocument(new BsonElement("OffsetDateTime", "bleh"))))
            .Throws<FormatException>();
        Wrap.It(
                () =>
                    BsonSerializer.Deserialize<Test>(
                        new BsonDocument(new BsonElement("OffsetDateTime", BsonNull.Value))
                    )
            )
            .Throws<FormatException>();
    }

    [Fact]
    public void CanParseNullable()
    {
        BsonSerializer
            .Deserialize<Test>(new BsonDocument(new BsonElement("NullableOffsetDateTime", BsonNull.Value)))
            .NullableOffsetDateTime.IsDefault();
    }

    private class Test
    {
        public OffsetDateTime OffsetDateTime { get; set; }

        public OffsetDateTime? NullableOffsetDateTime { get; set; }
    }
}
