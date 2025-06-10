using NodaTime;
using NodaTime.Text;

namespace Annium.MongoDb.NodaTime;

/// <summary>
/// BSON serializer for NodaTime Period values using normalizing ISO pattern
/// </summary>
public class PeriodSerializer : PatternSerializer<Period>
{
    /// <summary>
    /// Initializes a new instance of the PeriodSerializer class
    /// </summary>
    public PeriodSerializer()
        : base(PeriodPattern.NormalizingIso) { }
}
