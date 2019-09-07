using Annium.Logging.Abstractions;
using Annium.Logging.InMemory;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class LogRouteExtensions
    {
        public static LogRoute UseInMemory(this LogRoute route, InMemoryLogHandler handler = null)
        {
            route.Use(handler is null ?
                ServiceDescriptor.Singleton<InMemoryLogHandler, InMemoryLogHandler>() :
                ServiceDescriptor.Singleton<InMemoryLogHandler>(handler)
            );

            return route;
        }
    }
}