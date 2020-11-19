using System;
using Annium.Core.Primitives;
using Annium.Logging.Abstractions;
using Annium.Logging.Console;

namespace Annium.Core.DependencyInjection
{
    public static class LogRouteExtensions
    {
        public static LogRoute UseConsole(
            this LogRoute route,
            bool color = false
        ) => route.UseConsole(DefaultFormat, color);

        public static LogRoute UseConsole(
            this LogRoute route,
            Func<LogMessage, string, string> format,
            bool color = false
        )
        {
            route.Use(Microsoft.Extensions.DependencyInjection.ServiceDescriptor.Singleton(new ConsoleLogHandler(format, color)));

            return route;
        }

        private static string DefaultFormat(LogMessage m, string message) =>
            $"[{m.Instant.InZone(ConsoleLogHandler.Tz).LocalDateTime.ToString("HH:mm:ss.fff", null)}] {m.Level} - {m.Source.FriendlyName()}: {message}";
    }
}