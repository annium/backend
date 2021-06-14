using System;
using Annium.Core.Internal;
using Annium.Core.Primitives;
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
            route.Use(ServiceDescriptor.Instance(new ConsoleLogHandler(format, color), ServiceLifetime.Singleton));

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
            var subjectType = m.SubjectType;

            var caller = string.Empty;
            var callerType = m.Type.Contains('_') ? subjectType : m.Type;
            if (m.Line != 0)
                caller = $" at {callerType}.{m.Member}#{m.Line}:{m.Column}";

            var source = m.Source == subjectType || m.Source == callerType ? string.Empty : $" {m.Source}";
            var subject = subjectType is null ? $" {subjectType}#{m.SubjectId}" : string.Empty;
            var sourceSubject = new[] { source, subject }.Join(" - ");

            return $"[{time}] {m.Level} [{m.ThreadId:D3}]{sourceSubject}{caller} >> {message}";
        }
    }
}