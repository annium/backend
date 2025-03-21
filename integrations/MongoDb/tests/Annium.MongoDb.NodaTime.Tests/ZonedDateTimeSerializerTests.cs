﻿using System;
using Annium.Testing;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using NodaTime;
using Xunit;

namespace Annium.MongoDb.NodaTime.Tests;

public class ZonedDateTimeSerializerTests
{
    private static readonly DateTimeZone _easternTimezone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(
        "America/New_York"
    )!;

    static ZonedDateTimeSerializerTests()
    {
        BsonSerializer.RegisterSerializer(new ZonedDateTimeSerializer());
    }

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

    [Fact]
    public void ThrowsWhenDateIsInvalid()
    {
        Wrap.It(() => BsonSerializer.Deserialize<Test>(new BsonDocument(new BsonElement("ZonedDateTime", "bleh"))))
            .Throws<FormatException>();
        Wrap.It(
                () =>
                    BsonSerializer.Deserialize<Test>(new BsonDocument(new BsonElement("ZonedDateTime", BsonNull.Value)))
            )
            .Throws<FormatException>();
    }

    [Fact]
    public void CanParseNullable()
    {
        BsonSerializer
            .Deserialize<Test>(new BsonDocument(new BsonElement("NullableZonedDateTime", BsonNull.Value)))
            .NullableZonedDateTime.IsDefault();
    }

    private class Test
    {
        public ZonedDateTime ZonedDateTime { get; set; }

        public ZonedDateTime? NullableZonedDateTime { get; set; }
    }
}
