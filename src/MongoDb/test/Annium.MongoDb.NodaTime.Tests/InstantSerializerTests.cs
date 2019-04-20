using System;
using Annium.Testing;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using NodaTime;

namespace Annium.MongoDb.NodaTime.Tests
{
    public class InstantSerializerTests
    {
        static InstantSerializerTests()
        {
            BsonSerializer.RegisterSerializer(new InstantSerializer());
        }

        [Fact]
        public void CanConvertValid()
        {
            var instant = Instant.FromUtc(2015, 1, 1, 0, 0, 1);
            var obj = new Test { Instant = instant };
            obj.ToTestJson().Contains("'Instant' : ISODate('2015-01-01T00:00:01Z')").IsTrue();

            obj = BsonSerializer.Deserialize<Test>(obj.ToBson());
            obj.Instant.IsEqual(instant);
        }

        [Fact]
        public void CanConvertNullableValid()
        {
            var instant = Instant.FromUtc(2015, 1, 1, 0, 0, 1);
            var obj = new Test { InstantNullable = instant };
            obj.ToTestJson().Contains("'InstantNullable' : ISODate('2015-01-01T00:00:01Z')").IsTrue();

            obj = BsonSerializer.Deserialize<Test>(obj.ToBson());
            obj.InstantNullable.IsEqual(instant);
        }

        [Fact]
        public void CanConvertNullableNull()
        {
            var instant = Instant.FromUtc(2015, 1, 1, 0, 0, 1);
            var obj = new Test { Instant = instant };
            obj.ToTestJson().Contains("'Instant' : ISODate('2015-01-01T00:00:01Z')").IsTrue();
            obj.ToTestJson().Contains("'InstantNullable' : null").IsTrue();

            obj = BsonSerializer.Deserialize<Test>(obj.ToBson());
            obj.Instant.IsEqual(instant);
            obj.InstantNullable.IsDefault();
        }

        [Fact]
        public void SupportsOldFormat()
        {
            var instant = Instant.FromUtc(2015, 1, 1, 1, 0, 1);

            var doc = new BsonDocument(new BsonElement("Instant", "2015-01-01T01:00:01Z"));
            var obj = BsonSerializer.Deserialize<Test>(doc);
            obj.Instant.IsEqual(instant);
        }

        [Fact]
        public void ThrowsForInvalidTypes()
        {
            var doc = new BsonDocument(new BsonElement("Instant", new BsonBoolean(false)));
            ((Action) (() => BsonSerializer.Deserialize<Test>(doc))).Throws<FormatException>();

            doc = new BsonDocument(new BsonElement("Instant", new BsonInt32(1)));
            ((Action) (() => BsonSerializer.Deserialize<Test>(doc))).Throws<FormatException>();
        }

        [Fact]
        public void ThrowsForNullWhenNotNullable()
        {
            var doc = new BsonDocument(new BsonElement("Instant", BsonNull.Value));
            ((Action) (() => BsonSerializer.Deserialize<Test>(doc))).Throws<FormatException>();
        }

        [Fact]
        public void CanParseNullable()
        {
            BsonSerializer.Deserialize<Test>(new BsonDocument(new BsonElement("InstantNullable", BsonNull.Value))).InstantNullable.IsDefault();
        }

        private class Test
        {
            public Instant Instant { get; set; }

            public Instant? InstantNullable { get; set; }
        }
    }
}