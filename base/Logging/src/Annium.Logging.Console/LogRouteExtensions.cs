using Annium.Logging.Abstractions;
using Annium.Logging.Console;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class LogRouteExtensions
    {
        public static LogRoute UseConsole(this LogRoute route)
        {
            route.Use(ServiceDescriptor.Singleton<ConsoleLogHandler, ConsoleLogHandler>());

            return route;
        }
    }
}