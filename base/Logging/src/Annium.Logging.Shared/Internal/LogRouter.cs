using System;
using System.Collections.Generic;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime.Time;
using Annium.Logging.Abstractions;

namespace Annium.Logging.Shared.Internal
{
    internal class LogRouter : ILogRouter
    {
        private readonly IEnumerable<LogRoute> _routes;
        private readonly IServiceProvider _provider;
        private readonly ITimeProvider _timeProvider;
        private readonly IDictionary<LogRoute, ILogHandler> _handlers = new Dictionary<LogRoute, ILogHandler>();

        public LogRouter(
            ITimeProvider timeProvider,
            IEnumerable<LogRoute> routes,
            IServiceProvider provider
        )
        {
            _routes = routes;
            _provider = provider;
            _timeProvider = timeProvider;
        }

        public void Send(LogLevel level, Type source, string message, Exception? exception)
        {
            var instant = _timeProvider.Now;
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