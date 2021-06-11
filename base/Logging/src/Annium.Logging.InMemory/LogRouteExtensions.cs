using Annium.Logging.Abstractions;
using Annium.Logging.InMemory;
using Annium.Logging.Shared;

namespace Annium.Core.DependencyInjection
{
    public static class LogRouteExtensions
    {
        public static LogRoute UseInMemory(this LogRoute route)
        {
            route.Use(ServiceDescriptor.Type<InMemoryLogHandler, InMemoryLogHandler>(ServiceLifetime.Singleton));

            return route;
        }

        public static LogRoute UseInMemory(this LogRoute route, InMemoryLogHandler handler)
        {
            route.Use(ServiceDescriptor.Instance(handler, ServiceLifetime.Singleton));

            return route;
        }
    }
}