using NodaTime;
using NodaTime.Text;

namespace Annium.MongoDb.NodaTime;

/// <summary>
/// BSON serializer for NodaTime Period values using the round-trip ISO pattern
/// </summary>
/// <remarks>
/// The normalizing pattern this used before re-expressed the value in other units before writing it:
/// ninety minutes was stored as one hour and thirty minutes. Period equality is unit-wise, and applying
/// a period to a date can give different answers depending on which units it carries, so a value did not
/// come back as the one that went in. Normalizing also summed the time components into nanoseconds, which
/// overflowed for periods a caller could legitimately construct. Values written by the earlier pattern
/// still read: both are ISO-8601 durations.
/// </remarks>
public class PeriodSerializer : PatternSerializer<Period>
{
    /// <summary>
    /// Initializes a new instance of the PeriodSerializer class
    /// </summary>
    public PeriodSerializer()
        : base(PeriodPattern.Roundtrip) { }
}
