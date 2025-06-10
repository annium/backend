using NodaTime;
using NodaTime.Text;

namespace Annium.MongoDb.NodaTime;

/// <summary>
/// BSON serializer for NodaTime LocalTime values using extended ISO pattern
/// </summary>
public class LocalTimeSerializer : PatternSerializer<LocalTime>
{
    /// <summary>
    /// Initializes a new instance of the LocalTimeSerializer class
    /// </summary>
    public LocalTimeSerializer()
        : base(LocalTimePattern.ExtendedIso) { }
}
