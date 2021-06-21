using System;
using System.Text;
using Annium.Core.Internal;
using Annium.Logging.Console;
using Annium.Logging.Shared;

namespace Annium.Core.DependencyInjection
{
    public static class LogRouteExtensions
    {
        public static LogRoute UseConsole(
            this LogRoute route,
            bool color = false
        ) => route.UseConsole(DefaultFormat, color);

        public static LogRoute UseTestConsole(
            this LogRoute route,
            bool color = false
        ) => route.UseConsole(DefaultTestFormat, color);

        public static LogRoute UseConsole(
            this LogRoute route,
            Func<LogMessage, string, string> format,
            bool color = false
        )
        {
            route.UseInstance(new ConsoleLogHandler(format, color), new LogRouteConfiguration(0));

            return route;
        }

        private static string DefaultFormat(LogMessage m, string message)
        {
            var time = m.Instant.InZone(ConsoleLogHandler.Tz).LocalDateTime.ToString("HH:mm:ss.fff", null);
            return DefaultFormatInternal(m, time, message);
        }

        private static string DefaultTestFormat(LogMessage m, string message)
        {
            var time = Log.ToRelativeLogTime(m.Instant.InZone(ConsoleLogHandler.Tz).LocalDateTime.ToDateTimeUnspecified()).ToString("hh\\:mm\\:ss\\.fff");
            return DefaultFormatInternal(m, time, message);
        }

        private static string DefaultFormatInternal(LogMessage m, string time, string message)
        {
            var sb = new StringBuilder();
            sb.Append(string.IsNullOrWhiteSpace(m.SubjectType) ? m.Source : $"{m.SubjectType}#{m.SubjectId}");
            if (m.Line != 0)
                sb.Append($" at {m.Type}.{m.Member}:{m.Line}");

            return $"[{time}] {m.Level} [{m.ThreadId:D3}] {sb} >> {message}";
        }
    }
}