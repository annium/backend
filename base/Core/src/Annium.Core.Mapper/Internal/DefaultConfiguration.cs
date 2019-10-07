using System;
using NodaTime;

namespace Annium.Core.Mapper.Internal
{
    internal class DefaultConfiguration : MapperConfiguration
    {
        public DefaultConfiguration()
        {
            Map<DateTime, Instant>(d => Instant.FromDateTimeUtc(d.ToUniversalTime()));
            Map<Instant, DateTime>(i => i.ToDateTimeUtc());
            Map<DateTimeOffset, Instant>(d => Instant.FromDateTimeOffset(d));
            Map<Instant, DateTimeOffset>(i => i.ToDateTimeOffset());

            // string -> xxx
            Map<string, bool>(s => bool.Parse(s));
            Map<string, int>(s => int.Parse(s));
            Map<string, uint>(s => uint.Parse(s));
            Map<string, long>(s => long.Parse(s));
            Map<string, ulong>(s => ulong.Parse(s));
            Map<string, float>(s => float.Parse(s));
            Map<string, double>(s => double.Parse(s));
            Map<string, decimal>(s => decimal.Parse(s));
            Map<string, DateTime>(s => DateTime.Parse(s));
            Map<string, DateTimeOffset>(s => DateTimeOffset.Parse(s));
            Map<string, Guid>(s => Guid.Parse(s));
            Map<string, Uri>(s => new Uri(s));

            // xxx -> string
            Map<bool, string>(v => v.ToString());
            Map<int, string>(v => v.ToString());
            Map<uint, string>(v => v.ToString());
            Map<long, string>(v => v.ToString());
            Map<ulong, string>(v => v.ToString());
            Map<float, string>(v => v.ToString());
            Map<double, string>(v => v.ToString());
            Map<decimal, string>(v => v.ToString());
            Map<DateTime, string>(v => v.ToString());
            Map<DateTimeOffset, string>(v => v.ToString());
            Map<Guid, string>(v => v.ToString());
            Map<Uri, string>(v => v.ToString());
        }
    }
}