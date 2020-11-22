using System;
using System.Collections.Generic;
using Annium.Core.DependencyInjection;
using NodaTime;

namespace Annium.Logging.Abstractions.Internal
{
    internal class LogRouter : ILogRouter
    {
        private readonly IEnumerable<LogRoute> _routes;
        private readonly IServiceProvider _provider;
        private readonly Func<Instant> _getInstant;
        private readonly IDictionary<LogRoute, ILogHandler> _handlers = new Dictionary<LogRoute, ILogHandler>();

        public LogRouter(
            Func<Instant> getInstant,
            IEnumerable<LogRoute> routes,
            IServiceProvider provider
        )
        {
            _routes = routes;
            _provider = provider;
            _getInstant = getInstant;
        }

        public void Send(LogLevel level, Type source, string message, Exception? exception)
        {
            var instant = _getInstant();
            var msg = new LogMessage(instant, level, source, message, exception);

            foreach (var route in _routes)
                if (route.Filter(msg))
                    GetHandler(route).Handle(msg);
        }

        private ILogHandler GetHandler(LogRoute route)
        {
            lock (_handlers)
            {
                if (_handlers.TryGetValue(route, out var handler))
                    return handler;

                return _handlers[route] = (ILogHandler) _provider.Resolve(route.Service!.ServiceType);
            }
        }
    }
}