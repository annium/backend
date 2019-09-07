using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace Annium.Logging.Abstractions
{
    internal class LogRouter
    {
        private readonly IEnumerable<LogRoute> routes;
        private readonly IServiceProvider provider;
        private readonly Func<Instant> getInstant;
        private readonly IDictionary<LogRoute, ILogHandler> handlers = new Dictionary<LogRoute, ILogHandler>();

        public LogRouter(
            Func<Instant> getInstant,
            IEnumerable<LogRoute> routes,
            IServiceProvider provider
        )
        {
            this.routes = routes;
            this.provider = provider;
            this.getInstant = getInstant;
        }

        public void Send(LogLevel level, Type source, string message, Exception exception)
        {
            var instant = getInstant();
            var msg = new LogMessage(instant, level, source, message, exception);

            foreach (var route in routes)
                if (route.Filter(msg))
                    GetHandler(route).Handle(msg);
        }

        private ILogHandler GetHandler(LogRoute route)
        {
            lock(handlers)
            {
                if (handlers.TryGetValue(route, out var handler))
                    return handler;

                return handlers[route] = (ILogHandler) provider.GetRequiredService(route.Service.ServiceType);
            }
        }
    }
}