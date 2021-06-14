using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Annium.Core.DependencyInjection;
using Annium.Core.Primitives;
using Annium.Core.Runtime.Time;
using Annium.Diagnostics.Debug;
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

        public void Send(
            ILogSubject subject,
            LogLevel level,
            string source,
            string message,
            Exception? exception,
            object[] data
        )
        {
            var instant = _timeProvider.Now;
            var frame = EnhancedStackTrace.Current().GetFrame(3);
            var method = frame.GetMethod();
            var type = (method.ReflectedType ?? method.DeclaringType ?? throw new InvalidOperationException()).FriendlyName();
            var member = method.IsSpecialName
                ? method.Name.StartsWith("get_") ? method.Name.Replace("get_", string.Empty) :
                method.Name.StartsWith("get_") ? method.Name.Replace("get_", string.Empty) : method.Name
                : method.Name;

            var msg = new LogMessage(
                instant,
                subject.GetType().FriendlyName(),
                subject.GetId(),
                level,
                source,
                Thread.CurrentThread.ManagedThreadId,
                message,
                exception,
                data,
                type,
                member,
                frame.GetFileLineNumber(),
                frame.GetFileColumnNumber()
            );

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