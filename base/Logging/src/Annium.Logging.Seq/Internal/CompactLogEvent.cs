using System.Collections.Generic;
using System.Text;
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
            var prefix = BuildMessagePrefix(m);
            var result = new Dictionary<string, string>
            {
                ["@p"] = project,
                ["@t"] = m.Instant.InUtc().LocalDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fff'Z'", null),
                ["@m"] = $"{prefix}{message}",
                ["@mt"] = $"{prefix}{m.MessageTemplate}",
                ["@l"] = m.Level.ToString(),
            };
            if (m.Exception is not null)
                result["@x"] = $"{m.Exception.Message}{m.Exception.StackTrace}";

            foreach (var (key, value) in m.Data)
                result[key] = value.ToString();

            return result;
        }

        private static string BuildMessagePrefix(LogMessage m)
        {
            var sb = new StringBuilder();
            sb.Append(string.IsNullOrWhiteSpace(m.SubjectType) ? m.Source : $"{m.SubjectType}#{m.SubjectId}");
            if (m.Line != 0)
                sb.Append($" at {m.Type}.{m.Member}:{m.Line}");

            return $"[{m.ThreadId:D3}] {sb} >> ";
        }
    }
}