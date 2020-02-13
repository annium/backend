using Annium.Logging.Abstractions;
using Annium.Logging.Console;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class LogRouteExtensions
    {
        public static LogRoute UseConsole(
            this LogRoute route,
            bool time = false,
            bool level = false,
            bool color = false
        )
        {
            route.Use(ServiceDescriptor.Singleton(new ConsoleLogHandler(time, level, color)));

            return route;
        }
    }
}