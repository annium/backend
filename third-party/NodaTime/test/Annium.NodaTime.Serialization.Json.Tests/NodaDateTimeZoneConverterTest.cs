using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Annium.Testing;
using NodaTime;
using NodaTime.TimeZones;
using static Annium.NodaTime.Serialization.Json.Tests.TestHelper;

namespace Annium.NodaTime.Serialization.Json.Tests
{
    public class NodaDateTimeZoneConverterTest
    {
        private readonly JsonConverter converter = Converters.CreateDateTimeZoneConverter(DateTimeZoneProviders.Tzdb);

        [Fact]
        public void Serialize()
        {
            var dateTimeZone = DateTimeZoneProviders.Tzdb["America/Los_Angeles"];
            var json = JsonSerializer.Serialize(dateTimeZone, With(converter));
            json.IsEqual("\"America/Los_Angeles\"");
        }

        [Fact]
        public void Deserialize()
        {
            string json = "\"America/Los_Angeles\"";
            var dateTimeZone = JsonSerializer.Deserialize<DateTimeZone>(json, With(converter));
            var expectedDateTimeZone = DateTimeZoneProviders.Tzdb["America/Los_Angeles"];
            dateTimeZone.IsEqual(expectedDateTimeZone);
        }

        [Fact]
        public void Deserialize_TimeZoneNotFound()
        {
            string json = "\"America/DOES_NOT_EXIST\"";
            ((Action) (() => JsonSerializer.Deserialize<DateTimeZone>(json, With(converter))))
            .Throws<JsonException>();
        }
    }
}