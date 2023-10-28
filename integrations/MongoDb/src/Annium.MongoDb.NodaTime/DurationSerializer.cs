using NodaTime;
using NodaTime.Text;

namespace Annium.MongoDb.NodaTime;

public class DurationSerializer : PatternSerializer<Duration>
{
    public DurationSerializer()
        : base(DurationPattern.Roundtrip) { }
}
