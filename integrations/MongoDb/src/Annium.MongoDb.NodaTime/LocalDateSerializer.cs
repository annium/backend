using NodaTime;
using NodaTime.Text;

namespace Annium.MongoDb.NodaTime;

/// <summary>
/// BSON serializer for NodaTime LocalDate values using ISO pattern with calendar normalization
/// </summary>
public class LocalDateSerializer : PatternSerializer<LocalDate>
{
    /// <summary>
    /// Initializes a new instance of the LocalDateSerializer class
    /// </summary>
    public LocalDateSerializer()
        : base(LocalDatePattern.Iso, d => d.WithCalendar(CalendarSystem.Iso)) { }
}
