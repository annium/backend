using NodaTime;
using NodaTime.Text;

namespace Annium.MongoDb.NodaTime;

public class LocalTimeSerializer : PatternSerializer<LocalTime>
{
    public LocalTimeSerializer()
        : base(LocalTimePattern.ExtendedIso) { }
}
