using System;
using Annium.Testing;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using NodaTime;
using Xunit;

namespace Annium.MongoDb.NodaTime.Tests;

/// <summary>
/// Tests for the Period serializer functionality
/// </summary>
public class PeriodSerializerTests
{
    /// <summary>
    /// Static constructor registering the package's serializers, the same way a consumer does. The
    /// registry is process-wide, so every class here goes through the one entry point rather than
    /// registering its own - two classes registering different instances for one type is a conflict.
    /// Registers the Period serializer
    /// </summary>
    static PeriodSerializerTests()
    {
        NodaTimeSerializers.Register();
    }

    /// <summary>
    /// Tests that Period values can be serialized and deserialized correctly
    /// </summary>
    [Fact]
    public void CanConvertValue()
    {
        var obj = new Test { Period = Period.FromSeconds(34) };
        obj.ToTestJson().Contains("'Period' : 'PT34S'").IsTrue();

        obj = BsonSerializer.Deserialize<Test>(obj.ToBson());
        obj.Period.Is(Period.FromSeconds(34));
    }

    /// <summary>
    /// A period keeps the units it was built from. Period equality is unit-wise - ninety minutes and one
    /// hour thirty are different periods, and applying them to a date can give different answers - so a
    /// round trip that re-expresses the value in other units returns something that is not what went in.
    /// </summary>
    [Fact]
    public void CanRoundTripValue_KeepingItsUnits()
    {
        var period = Period.FromMinutes(90);
        var obj = new Test { Period = period };

        obj = BsonSerializer.Deserialize<Test>(obj.ToBson());

        obj.Period.Is(period, "the period must come back in the units it was written in");
    }

    /// <summary>
    /// A period too large to express as a single nanosecond count still writes. Normalizing summed the
    /// time components before formatting, so a value the caller was allowed to construct threw on the way
    /// to storage rather than being stored.
    /// </summary>
    [Fact]
    public void CanWriteValue_TooLargeToNormalize()
    {
        var period = Period.FromHours(long.MaxValue / 2);
        var obj = new Test { Period = period };

        obj = BsonSerializer.Deserialize<Test>(obj.ToBson());

        obj.Period.Is(period);
    }

    /// <summary>
    /// Tests that deserialization throws FormatException for invalid Period strings
    /// </summary>
    [Fact]
    public void ThrowsWhenValueIsInvalid()
    {
        Wrap.It(() => BsonSerializer.Deserialize<Test>(new BsonDocument(new BsonElement("Period", "bleh"))))
            .Throws<FormatException>();
    }

    /// <summary>
    /// Tests that Period properties can handle null BSON values when defaulted to Period.Zero
    /// </summary>
    [Fact]
    public void CanParseNullable()
    {
        BsonSerializer
            .Deserialize<Test>(new BsonDocument(new BsonElement("Period", BsonNull.Value)))
            .Period.IsDefault();
    }

    /// <summary>
    /// Test class containing Period properties for serialization testing
    /// </summary>
    private class Test
    {
        /// <summary>
        /// Gets or sets a Period value, defaults to Period.Zero
        /// </summary>
        public Period Period { get; set; } = Period.Zero;
    }
}
