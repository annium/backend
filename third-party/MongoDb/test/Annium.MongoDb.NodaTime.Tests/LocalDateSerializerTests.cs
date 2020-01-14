using System;
using Annium.Testing;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using NodaTime;
using Xunit;

namespace Annium.MongoDb.NodaTime.Tests
{
    public class LocalDateSerializerTests
    {
        static LocalDateSerializerTests()
        {
            BsonSerializer.RegisterSerializer(new LocalDateSerializer());
        }

        [Fact]
        public void CanRoundTripValueWithIsoCalendar()
        {
            var obj = new Test { LocalDate = new LocalDate(2015, 1, 1) };
            obj.ToTestJson().Contains("'LocalDate' : '2015-01-01'").IsTrue();

            obj = BsonSerializer.Deserialize<Test>(obj.ToBson());
            obj.LocalDate.IsEqual(obj.LocalDate);
        }

        [Fact]
        public void ConvertsToIsoCalendarWhenSerializing()
        {
            var obj = new Test { LocalDate = new LocalDate(2015, 1, 1).WithCalendar(CalendarSystem.PersianSimple) };
            obj.ToTestJson().Contains("'LocalDate' : '2015-01-01'").IsTrue();

            obj = BsonSerializer.Deserialize<Test>(obj.ToBson());
            obj.LocalDate.IsEqual(obj.LocalDate.WithCalendar(CalendarSystem.Iso));
        }

        [Fact]
        public void ThrowsWhenDateIsInvalid()
        {
            ((Action) (() => BsonSerializer.Deserialize<Test>(new BsonDocument(new BsonElement("LocalDate", "bleh"))))).Throws<FormatException>();
            ((Action) (() => BsonSerializer.Deserialize<Test>(new BsonDocument(new BsonElement("LocalDate", BsonNull.Value))))).Throws<FormatException>();
        }

        [Fact]
        public void CanParseNullable()
        {
            BsonSerializer.Deserialize<Test>(new BsonDocument(new BsonElement("NullableLocalDate", BsonNull.Value))).NullableLocalDate.IsDefault();
        }

        private class Test
        {
            public LocalDate LocalDate { get; set; }

            public LocalDate? NullableLocalDate { get; set; }
        }
    }
}