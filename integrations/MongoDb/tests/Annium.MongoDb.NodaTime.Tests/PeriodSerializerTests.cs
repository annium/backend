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
    /// Static constructor to register the Period serializer
    /// </summary>
    static PeriodSerializerTests()
    {
        BsonSerializer.RegisterSerializer(new PeriodSerializer());
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
        obj.Period.Is(obj.Period);
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
