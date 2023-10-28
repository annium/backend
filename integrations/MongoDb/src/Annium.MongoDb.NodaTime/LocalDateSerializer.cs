using NodaTime;
using NodaTime.Text;

namespace Annium.MongoDb.NodaTime;

public class LocalDateSerializer : PatternSerializer<LocalDate>
{
    public LocalDateSerializer()
        : base(LocalDatePattern.Iso, d => d.WithCalendar(CalendarSystem.Iso)) { }
}
