using System.Collections.Generic;
using Annium.Logging.Shared;
using NodaTime;

namespace Annium.Logging.Seq.Internal
{
    internal static class CompactLogEvent
    {
        public static IReadOnlyDictionary<string, string> Format(
            string project,
            LogMessage m,
            string message,
            DateTimeZone tz
        )
        {
            var result = new Dictionary<string, string>
            {
                ["@p"] = project,
                ["@t"] = m.Instant.InUtc().LocalDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fff'Z'", null),
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