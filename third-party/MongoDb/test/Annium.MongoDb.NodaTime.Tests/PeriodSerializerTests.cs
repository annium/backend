using System;
using Annium.Testing;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using NodaTime;
using Xunit;

namespace Annium.MongoDb.NodaTime.Tests;

public class PeriodSerializerTests
{
    static PeriodSerializerTests()
    {
        BsonSerializer.RegisterSerializer(new PeriodSerializer());
    }

    [Fact]
    public void CanConvertValue()
    {
        var obj = new Test { Period = Period.FromSeconds(34) };
        obj.ToTestJson().Contains("'Period' : 'PT34S'").IsTrue();

        obj = BsonSerializer.Deserialize<Test>(obj.ToBson());
        obj.Period.IsEqual(obj.Period);
    }

    [Fact]
    public void ThrowsWhenValueIsInvalid()
    {
        ((Action) (() => BsonSerializer.Deserialize<Test>(new BsonDocument(new BsonElement("Period", "bleh"))))).Throws<FormatException>();
    }

    [Fact]
    public void CanParseNullable()
    {
        BsonSerializer.Deserialize<Test>(new BsonDocument(new BsonElement("Period", BsonNull.Value))).Period.IsDefault();
    }

    private class Test
    {
        public Period Period { get; set; } = Period.Zero;
    }
}