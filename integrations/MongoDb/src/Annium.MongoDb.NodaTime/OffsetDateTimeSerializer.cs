using NodaTime;
using NodaTime.Text;

namespace Annium.MongoDb.NodaTime;

/// <summary>
/// BSON serializer for NodaTime OffsetDateTime values using extended ISO pattern with calendar normalization
/// </summary>
public class OffsetDateTimeSerializer : PatternSerializer<OffsetDateTime>
{
    /// <summary>
    /// Initializes a new instance of the OffsetDateTimeSerializer class
    /// </summary>
    public OffsetDateTimeSerializer()
        : base(OffsetDateTimePattern.ExtendedIso, d => d.WithCalendar(CalendarSystem.Iso)) { }
}
