using Annium.Logging.Abstractions;
using Annium.Logging.InMemory;

namespace Annium.Core.DependencyInjection
{
    public static class LogRouteExtensions
    {
        public static LogRoute UseInMemory(this LogRoute route)
        {
            route.Use(Microsoft.Extensions.DependencyInjection.ServiceDescriptor.Singleton<InMemoryLogHandler, InMemoryLogHandler>());

            return route;
        }

        public static LogRoute UseInMemory(this LogRoute route, InMemoryLogHandler handler)
        {
            route.Use(Microsoft.Extensions.DependencyInjection.ServiceDescriptor.Singleton(handler));

            return route;
        }
    }
}