using System;
using Annium.Logging.Console.Internal;
using Annium.Logging.Shared;

namespace Annium.Core.DependencyInjection
{
    public static class LogRouteExtensions
    {
        public static LogRoute<TContext> UseConsole<TContext>(
            this LogRoute<TContext> route,
            bool color = false
        )
            where TContext : class, ILogContext
            => route.UseConsole(LogMessageExtensions.DefaultFormat, color);

        public static LogRoute<TContext> UseTestConsole<TContext>(
            this LogRoute<TContext> route,
            bool color = false
        )
            where TContext : class, ILogContext
            => route.UseConsole(LogMessageExtensions.DefaultTestFormat, color);

        public static LogRoute<TContext> UseConsole<TContext>(
            this LogRoute<TContext> route,
            Func<LogMessage<TContext>, string, string> format,
            bool color = false
        )
            where TContext : class, ILogContext
        {
            route.UseInstance(new ConsoleLogHandler<TContext>(format, color), new LogRouteConfiguration());

            return route;
        }
    }
}