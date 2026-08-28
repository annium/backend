using Annium.Testing;
using MongoDB.Bson.Serialization;
using NodaTime;
using Xunit;

namespace Annium.MongoDb.NodaTime.Tests;

/// <summary>
/// Tests for the registration entry point every consumer of this package calls. It finds the serializers
/// by reflection rather than by a list, so a serializer added or renamed later is picked up - or quietly
/// missed - without anything else changing.
/// </summary>
public class NodaTimeSerializersTests
{
    /// <summary>
    /// Every NodaTime type this package serializes is wired up by the single call consumers make.
    /// </summary>
    [Fact]
    public void Register_WiresUpEverySerializer()
    {
        // act
        NodaTimeSerializers.Register();

        // assert
        BsonSerializer.LookupSerializer<Duration>().As<DurationSerializer>();
        BsonSerializer.LookupSerializer<Instant>().As<InstantSerializer>();
        BsonSerializer.LookupSerializer<LocalDate>().As<LocalDateSerializer>();
        BsonSerializer.LookupSerializer<LocalDateTime>().As<LocalDateTimeSerializer>();
        BsonSerializer.LookupSerializer<LocalTime>().As<LocalTimeSerializer>();
        BsonSerializer.LookupSerializer<OffsetDateTime>().As<OffsetDateTimeSerializer>();
        BsonSerializer.LookupSerializer<Period>().As<PeriodSerializer>();
        BsonSerializer.LookupSerializer<ZonedDateTime>().As<ZonedDateTimeSerializer>();
    }

    /// <summary>
    /// Registering twice is not an error. The driver refuses a second registration for a type and there is
    /// no way to ask it what is already registered, so the call swallows that refusal - which means a host
    /// that registers on each of several startup paths must not fall over on the second one.
    /// </summary>
    [Fact]
    public void Register_CalledTwice_DoesNotThrow()
    {
        // act - a second call would throw out of this test if the refusal were not swallowed
        NodaTimeSerializers.Register();
        NodaTimeSerializers.Register();

        // assert - and the registrations are still the ones this package made
        BsonSerializer.LookupSerializer<Instant>().As<InstantSerializer>();
    }
}
