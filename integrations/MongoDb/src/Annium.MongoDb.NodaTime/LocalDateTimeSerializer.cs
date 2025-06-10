using NodaTime;
using NodaTime.Text;

namespace Annium.MongoDb.NodaTime;

/// <summary>
/// BSON serializer for NodaTime LocalDateTime values using extended ISO pattern with calendar normalization
/// </summary>
public class LocalDateTimeSerializer : PatternSerializer<LocalDateTime>
{
    /// <summary>
    /// Initializes a new instance of the LocalDateTimeSerializer class
    /// </summary>
    public LocalDateTimeSerializer()
        : base(LocalDateTimePattern.ExtendedIso, d => d.WithCalendar(CalendarSystem.Iso)) { }
}
