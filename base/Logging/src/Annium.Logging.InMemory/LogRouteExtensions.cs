using Annium.Logging.InMemory;
using Annium.Logging.Shared;

namespace Annium.Core.DependencyInjection;

public static class LogRouteExtensions
{
    public static LogRoute<TContext> UseInMemory<TContext>(this LogRoute<TContext> route)
        where TContext : class, ILogContext
    {
        route.UseType<InMemoryLogHandler<TContext>>(new LogRouteConfiguration());

        return route;
    }

    public static LogRoute<TContext> UseInMemory<TContext>(this LogRoute<TContext> route, InMemoryLogHandler<TContext> handler)
        where TContext : class, ILogContext
    {
        route.UseInstance(handler, new LogRouteConfiguration());

        return route;
    }
}