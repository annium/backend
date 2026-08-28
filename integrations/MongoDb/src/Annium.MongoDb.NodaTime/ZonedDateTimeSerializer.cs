using NodaTime;
using NodaTime.Text;

namespace Annium.MongoDb.NodaTime;

/// <summary>
/// BSON serializer for NodaTime ZonedDateTime values using invariant culture pattern with TZDB provider
/// </summary>
public class ZonedDateTimeSerializer : PatternSerializer<ZonedDateTime>
{
    /// <summary>
    /// The pattern used for serializing ZonedDateTime values.
    /// </summary>
    /// <remarks>
    /// The general pattern this used before has no fractional-second component, so a sub-second value came
    /// back as a different instant with nothing to say it had been rounded - while every sibling serializer
    /// here keeps its fraction through an extended ISO pattern. The fraction is optional on the way in, so
    /// documents written by the earlier format still read.
    /// </remarks>
    private static readonly IPattern<ZonedDateTime> _pattern = ZonedDateTimePattern.CreateWithInvariantCulture(
        "uuuu'-'MM'-'dd'T'HH':'mm':'ss;FFFFFFFFF z '('o<g>')'",
        DateTimeZoneProviders.Tzdb
    );

    /// <summary>
    /// Initializes a new instance of the ZonedDateTimeSerializer class
    /// </summary>
    public ZonedDateTimeSerializer()
        : base(_pattern, d => d.WithCalendar(CalendarSystem.Iso)) { }
}
