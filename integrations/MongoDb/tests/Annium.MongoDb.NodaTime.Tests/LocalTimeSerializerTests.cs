using System;
using Annium.Testing;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using NodaTime;
using Xunit;

namespace Annium.MongoDb.NodaTime.Tests;

public class LocalTimeSerializerTests
{
    static LocalTimeSerializerTests()
    {
        BsonSerializer.RegisterSerializer(new LocalTimeSerializer());
    }

    [Fact]
    public void CanRoundTripValueWithIsoCalendar()
    {
        var obj = new Test { LocalTime = new LocalTime(13, 25, 1) };
        obj.ToTestJson().Contains("'LocalTime' : '13:25:01'").IsTrue();

        obj = BsonSerializer.Deserialize<Test>(obj.ToBson());
        obj.LocalTime.Is(obj.LocalTime);
    }

    [Fact]
    public void ThrowsWhenDateIsInvalid()
    {
        Wrap.It(() => BsonSerializer.Deserialize<Test>(new BsonDocument(new BsonElement("LocalTime", "bleh"))))
            .Throws<FormatException>();
        Wrap.It(() => BsonSerializer.Deserialize<Test>(new BsonDocument(new BsonElement("LocalTime", BsonNull.Value))))
            .Throws<FormatException>();
    }

    [Fact]
    public void CanParseNullable()
    {
        BsonSerializer
            .Deserialize<Test>(new BsonDocument(new BsonElement("NullableLocalTime", BsonNull.Value)))
            .NullableLocalTime.IsDefault();
    }

    private class Test
    {
        public LocalTime LocalTime { get; set; }

        public LocalTime? NullableLocalTime { get; set; }
    }
}
