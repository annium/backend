using NodaTime;
using NodaTime.Text;

namespace Annium.MongoDb.NodaTime
{
    public class PeriodSerializer : PatternSerializer<Period>
    {
        public PeriodSerializer() : base(PeriodPattern.NormalizingIso)
        {
        }
    }
}