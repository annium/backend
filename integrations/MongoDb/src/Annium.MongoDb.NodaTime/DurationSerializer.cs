using NodaTime;
using NodaTime.Text;

namespace Annium.MongoDb.NodaTime;

/// <summary>
/// BSON serializer for NodaTime Duration values using the roundtrip pattern
/// </summary>
public class DurationSerializer : PatternSerializer<Duration>
{
    /// <summary>
    /// Initializes a new instance of the DurationSerializer class
    /// </summary>
    public DurationSerializer()
        : base(DurationPattern.Roundtrip) { }
}
