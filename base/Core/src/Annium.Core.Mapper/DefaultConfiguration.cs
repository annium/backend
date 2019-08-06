using System;
using NodaTime;

namespace Annium.Core.Mapper
{
    internal static class DefaultConfiguration
    {
        public static void Apply(MapperConfiguration cfg)
        {
            cfg.Map<DateTime, Instant>(d => Instant.FromDateTimeUtc(d.ToUniversalTime()));
            cfg.Map<Instant, DateTime>(i => i.ToDateTimeUtc());

            // string -> xxx
            cfg.Map<string, bool>(s => bool.Parse(s));
            cfg.Map<string, int>(s => int.Parse(s));
            cfg.Map<string, uint>(s => uint.Parse(s));
            cfg.Map<string, long>(s => long.Parse(s));
            cfg.Map<string, ulong>(s => ulong.Parse(s));
            cfg.Map<string, float>(s => float.Parse(s));
            cfg.Map<string, double>(s => double.Parse(s));
            cfg.Map<string, decimal>(s => decimal.Parse(s));
            cfg.Map<string, DateTime>(s => DateTime.Parse(s));
            cfg.Map<string, DateTimeOffset>(s => DateTimeOffset.Parse(s));
            cfg.Map<string, Guid>(s => Guid.Parse(s));
            cfg.Map<string, Uri>(s => new Uri(s));

            // xxx -> string
            cfg.Map<bool, string>(v => v.ToString());
            cfg.Map<int, string>(v => v.ToString());
            cfg.Map<uint, string>(v => v.ToString());
            cfg.Map<long, string>(v => v.ToString());
            cfg.Map<ulong, string>(v => v.ToString());
            cfg.Map<float, string>(v => v.ToString());
            cfg.Map<double, string>(v => v.ToString());
            cfg.Map<decimal, string>(v => v.ToString());
            cfg.Map<DateTime, string>(v => v.ToString());
            cfg.Map<DateTimeOffset, string>(v => v.ToString());
            cfg.Map<Guid, string>(v => v.ToString());
            cfg.Map<Uri, string>(v => v.ToString());
        }
    }
}