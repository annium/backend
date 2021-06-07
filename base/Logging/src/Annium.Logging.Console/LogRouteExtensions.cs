using System;
using Annium.Core.Primitives;
using Annium.Logging.Abstractions;
using Annium.Logging.Console;
using NodaTime;

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
        ) => route.UseConsole(DefaultTestFormat(SystemClock.Instance.GetCurrentInstant()), color);

        public static LogRoute UseConsole(
            this LogRoute route,
            Func<LogMessage, string, string> format,
            bool color = false
        )
        {
            route.Use(ServiceDescriptor.Instance(new ConsoleLogHandler(format, color), ServiceLifetime.Singleton));

            return route;
        }

        private static string DefaultFormat(LogMessage m, string message) =>
            $"[{m.Instant.InZone(ConsoleLogHandler.Tz).LocalDateTime.ToString("HH:mm:ss.fff", null)}] {m.Level} - {m.Source.FriendlyName()}: {message}";

        private static Func<LogMessage, string, string> DefaultTestFormat(Instant since) => (m, message) =>
            $"[{(m.Instant - since).ToString("HH:mm:ss.fff", null)}] {m.Level} - {m.Source.FriendlyName()}: {message}";
    }
}