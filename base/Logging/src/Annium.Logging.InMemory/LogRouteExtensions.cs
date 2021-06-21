using Annium.Logging.InMemory;
using Annium.Logging.Shared;

namespace Annium.Core.DependencyInjection
{
    public static class LogRouteExtensions
    {
        public static LogRoute UseInMemory(this LogRoute route)
        {
            route.UseType<InMemoryLogHandler>(new LogRouteConfiguration(0));

            return route;
        }

        public static LogRoute UseInMemory(this LogRoute route, InMemoryLogHandler handler)
        {
            route.UseInstance(handler, new LogRouteConfiguration(0));

            return route;
        }
    }
}