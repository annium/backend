using NodaTime;
using NodaTime.Text;

namespace Annium.MongoDb.NodaTime;

/// <summary>
/// BSON serializer for NodaTime ZonedDateTime values using invariant culture pattern with TZDB provider
/// </summary>
public class ZonedDateTimeSerializer : PatternSerializer<ZonedDateTime>
{
    /// <summary>
    /// The pattern used for serializing ZonedDateTime values
    /// </summary>
    private static readonly IPattern<ZonedDateTime> _pattern = ZonedDateTimePattern.CreateWithInvariantCulture(
        "G",
        DateTimeZoneProviders.Tzdb
    );

    /// <summary>
    /// Initializes a new instance of the ZonedDateTimeSerializer class
    /// </summary>
    public ZonedDateTimeSerializer()
        : base(_pattern) { }
}
