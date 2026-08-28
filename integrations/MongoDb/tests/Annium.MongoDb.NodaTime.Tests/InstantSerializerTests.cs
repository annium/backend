using System;
using Annium.Testing;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using NodaTime;
using Xunit;

namespace Annium.MongoDb.NodaTime.Tests;

/// <summary>
/// Tests for the Instant serializer functionality
/// </summary>
public class InstantSerializerTests
{
    /// <summary>
    /// Static constructor registering the package's serializers, the same way a consumer does. The
    /// registry is process-wide, so every class here goes through the one entry point rather than
    /// registering its own - two classes registering different instances for one type is a conflict.
    /// Registers the Instant serializer
    /// </summary>
    static InstantSerializerTests()
    {
        NodaTimeSerializers.Register();
    }

    /// <summary>
    /// Tests that valid Instant values can be serialized and deserialized correctly
    /// </summary>
    [Fact]
    public void CanConvertValid()
    {
        var instant = Instant.FromUtc(2015, 1, 1, 0, 0, 1);
        var obj = new Test { Instant = instant };
        obj.ToTestJson().Contains("'Instant' : { '$date' : '2015-01-01T00:00:01Z' }").IsTrue();

        obj = BsonSerializer.Deserialize<Test>(obj.ToBson());
        obj.Instant.Is(instant);
    }

    /// <summary>
    /// Tests that nullable Instant values can be serialized and deserialized correctly
    /// </summary>
    [Fact]
    public void CanConvertNullableValid()
    {
        var instant = Instant.FromUtc(2015, 1, 1, 0, 0, 1);
        var obj = new Test { InstantNullable = instant };
        obj.ToTestJson().Contains("'InstantNullable' : { '$date' : '2015-01-01T00:00:01Z' }").IsTrue();

        obj = BsonSerializer.Deserialize<Test>(obj.ToBson());
        obj.InstantNullable.Is(instant);
    }

    /// <summary>
    /// Tests that nullable Instant properties serialize to null when not set
    /// </summary>
    [Fact]
    public void CanConvertNullableNull()
    {
        var instant = Instant.FromUtc(2015, 1, 1, 0, 0, 1);
        var obj = new Test { Instant = instant };
        obj.ToTestJson().Contains("'Instant' : { '$date' : '2015-01-01T00:00:01Z' }").IsTrue();
        obj.ToTestJson().Contains("'InstantNullable' : null").IsTrue();

        obj = BsonSerializer.Deserialize<Test>(obj.ToBson());
        obj.Instant.Is(instant);
        obj.InstantNullable.IsDefault();
    }

    /// <summary>
    /// Tests that the serializer supports deserializing old string format for Instant values
    /// </summary>
    [Fact]
    public void SupportsOldFormat()
    {
        var instant = Instant.FromUtc(2015, 1, 1, 1, 0, 1);

        var doc = new BsonDocument(new BsonElement("Instant", "2015-01-01T01:00:01Z"));
        var obj = BsonSerializer.Deserialize<Test>(doc);
        obj.Instant.Is(instant);
    }

    /// <summary>
    /// Tests that deserialization throws FormatException for invalid BSON types
    /// </summary>
    [Fact]
    public void ThrowsForInvalidTypes()
    {
        var doc = new BsonDocument(new BsonElement("Instant", new BsonBoolean(false)));
        Wrap.It(() => BsonSerializer.Deserialize<Test>(doc)).Throws<FormatException>();

        var doc2 = new BsonDocument(new BsonElement("Instant", new BsonInt32(1)));
        Wrap.It(() => BsonSerializer.Deserialize<Test>(doc2)).Throws<FormatException>();
    }

    /// <summary>
    /// Tests that deserialization throws FormatException when null is provided for non-nullable Instant
    /// </summary>
    [Fact]
    public void ThrowsForNullWhenNotNullable()
    {
        var doc = new BsonDocument(new BsonElement("Instant", BsonNull.Value));
        Wrap.It(() => BsonSerializer.Deserialize<Test>(doc)).Throws<FormatException>();
    }

    /// <summary>
    /// Tests that nullable Instant values can be deserialized from null BSON values
    /// </summary>
    [Fact]
    public void CanParseNullable()
    {
        BsonSerializer
            .Deserialize<Test>(new BsonDocument(new BsonElement("InstantNullable", BsonNull.Value)))
            .InstantNullable.IsDefault();
    }

    /// <summary>
    /// An instant is stored as a BSON date, which is a count of milliseconds - so anything finer is not
    /// kept. This is a property of the storage type rather than of the conversion: writing instants as
    /// strings instead would preserve the fraction but stop them being comparable, sortable and indexable
    /// as dates on the server, which is what storing them as dates is for. Pinned so the limit is visible
    /// and cannot change unnoticed.
    /// </summary>
    [Fact]
    public void RoundTrip_KeepsMilliseconds_AndNoFiner()
    {
        var whole = Instant.FromUtc(2015, 1, 2, 3, 4) + Duration.FromMilliseconds(567);
        var finer = whole + Duration.FromNanoseconds(123456);

        var wholeBack = BsonSerializer.Deserialize<Test>(new Test { Instant = whole }.ToBson()).Instant;
        var finerBack = BsonSerializer.Deserialize<Test>(new Test { Instant = finer }.ToBson()).Instant;

        wholeBack.Is(whole, "millisecond precision must survive");
        finerBack.Is(whole, "anything finer than a millisecond is dropped by the storage type");
    }

    /// <summary>
    /// An instant before 1970 is dropped to the millisecond below it, the same direction as one after.
    /// Dividing the tick count truncates towards zero rather than downwards, which moves a pre-epoch
    /// value forwards instead of back - so the same instant is rounded one way on one side of 1970 and
    /// the other way on the other, and a stored ordering between two close values can invert.
    /// </summary>
    [Fact]
    public void RoundTrip_BeforeEpoch_RoundsDownLikeEverythingElse()
    {
        // arrange - half a millisecond either side of the epoch
        var before = Instant.FromUnixTimeTicks(-5000);
        var after = Instant.FromUnixTimeTicks(5000);

        // act
        var beforeBack = BsonSerializer.Deserialize<Test>(new Test { Instant = before }.ToBson()).Instant;
        var afterBack = BsonSerializer.Deserialize<Test>(new Test { Instant = after }.ToBson()).Instant;

        // assert
        afterBack.Is(Instant.FromUnixTimeMilliseconds(0), "after the epoch, the fraction is dropped");
        beforeBack.Is(Instant.FromUnixTimeMilliseconds(-1), "before it, the fraction must be dropped too");
    }

    /// <summary>
    /// A stored string that is not an instant is refused. This serializer is the only one here that reads
    /// two BSON types, and the string branch - the one that parses - had no test for what it does with a
    /// value it cannot parse, though every other serializer pins exactly that.
    /// </summary>
    [Fact]
    public void ThrowsForInvalidString()
    {
        Wrap.It(() => BsonSerializer.Deserialize<Test>(new BsonDocument(new BsonElement("Instant", "bleh"))))
            .Throws<FormatException>();
    }

    /// <summary>
    /// Test class containing Instant properties for serialization testing
    /// </summary>
    private class Test
    {
        /// <summary>
        /// Gets or sets an Instant value
        /// </summary>
        public Instant Instant { get; set; }

        /// <summary>
        /// Gets or sets a nullable Instant value
        /// </summary>
        public Instant? InstantNullable { get; set; }
    }
}
