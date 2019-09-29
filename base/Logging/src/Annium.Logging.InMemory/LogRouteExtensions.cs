using Annium.Logging.Abstractions;
using Annium.Logging.InMemory;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class LogRouteExtensions
    {
        public static LogRoute UseInMemory(this LogRoute route)
        {
            route.Use(ServiceDescriptor.Singleton<InMemoryLogHandler, InMemoryLogHandler>());

            return route;
        }

        public static LogRoute UseInMemory(this LogRoute route, InMemoryLogHandler handler)
        {
            route.Use(ServiceDescriptor.Singleton(handler));

            return route;
        }
    }
}