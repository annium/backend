using System;
using Annium.Testing;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using NodaTime;
using Xunit;

namespace Annium.MongoDb.NodaTime.Tests;

public class DurationSerializerTests
{
    static DurationSerializerTests()
    {
        BsonSerializer.RegisterSerializer(new DurationSerializer());
    }

    [Fact]
    public void CanConvertValue()
    {
        var obj = new Test { Duration = Duration.FromSeconds(34) };
        obj.ToTestJson().Contains("'Duration' : '0:00:00:34'").IsTrue();

        obj = BsonSerializer.Deserialize<Test>(obj.ToBson());
        obj.Duration.IsEqual(obj.Duration);
    }

    [Fact]
    public void ThrowsWhenValueIsInvalid()
    {
        Action act = () => BsonSerializer.Deserialize<Test>(new BsonDocument(new BsonElement("Duration", "bleh")));
        act.Throws<FormatException>();
    }

    [Fact]
    public void CanParseNullable()
    {
        BsonSerializer.Deserialize<Test>(new BsonDocument(new BsonElement("DurationNullable", BsonNull.Value))).DurationNullable.IsDefault();
    }

    private class Test
    {
        public Duration Duration { get; set; }

        public Duration? DurationNullable { get; set; }
    }
}