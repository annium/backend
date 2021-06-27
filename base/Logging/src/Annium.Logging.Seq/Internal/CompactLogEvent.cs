using System.Collections.Generic;
using Annium.Logging.Shared;
using NodaTime;

namespace Annium.Logging.Seq.Internal
{
    internal static class CompactLogEvent
    {
        public static IReadOnlyDictionary<string, string> Format(
            LogMessage m,
            string message,
            DateTimeZone tz
        )
        {
            var result = new Dictionary<string, string>
            {
                ["@t"] = m.Instant.InZone(tz).LocalDateTime.ToString("HH:mm:ss.fff", null),
                ["@m"] = message,
                ["@mt"] = m.MessageTemplate,
                ["@l"] = m.Level.ToString(),
            };
            if (m.Exception is not null)
                result["@x"] = $"{m.Exception.Message}{m.Exception.StackTrace}";

            foreach (var (key, value) in m.Data)
                result[key] = value.ToString();

            return result;
        }
    }
}