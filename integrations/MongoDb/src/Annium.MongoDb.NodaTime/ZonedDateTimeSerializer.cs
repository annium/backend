using NodaTime;
using NodaTime.Text;

namespace Annium.MongoDb.NodaTime;

public class ZonedDateTimeSerializer : PatternSerializer<ZonedDateTime>
{
    private static readonly IPattern<ZonedDateTime> _pattern = ZonedDateTimePattern.CreateWithInvariantCulture(
        "G",
        DateTimeZoneProviders.Tzdb
    );

    public ZonedDateTimeSerializer()
        : base(_pattern) { }
}
