using NodaTime;
using NodaTime.Text;

namespace Annium.MongoDb.NodaTime;

public class ZonedDateTimeSerializer : PatternSerializer<ZonedDateTime>
{
    private static readonly IPattern<ZonedDateTime> Pattern = ZonedDateTimePattern.CreateWithInvariantCulture(
        "G",
        DateTimeZoneProviders.Tzdb
    );

    public ZonedDateTimeSerializer()
        : base(Pattern) { }
}
