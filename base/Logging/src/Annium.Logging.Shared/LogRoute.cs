using System;
using Annium.Core.DependencyInjection;

namespace Annium.Logging.Shared
{
    public class LogRoute
    {
        internal Func<LogMessage, bool> Filter { get; private set; } = _ => true;
        internal IServiceDescriptor? Service { get; private set; }
        internal LogRouteConfiguration? Configuration { get; private set; }
        private readonly Action<LogRoute> _registerRoute;

        internal LogRoute(Action<LogRoute> registerRoute)
        {
            _registerRoute = registerRoute;

            registerRoute(this);
        }

        public LogRoute For(Func<LogMessage, bool> filter) => new(_registerRoute) { Filter = filter };

        public LogRoute UseType<T>(LogRouteConfiguration configuration)
            where T : ILogHandler
            => Use(ServiceDescriptor.Type<T, T>(ServiceLifetime.Singleton), configuration);

        public LogRoute UseInstance(ILogHandler instance, LogRouteConfiguration configuration)
            => Use(ServiceDescriptor.Instance(instance, ServiceLifetime.Singleton), configuration);

        public LogRoute UseAsyncType<T>(LogRouteConfiguration configuration)
            where T : IAsyncLogHandler
            => Use(ServiceDescriptor.Type<T, T>(ServiceLifetime.Singleton), configuration);

        public LogRoute UseAsyncInstance(IAsyncLogHandler instance, LogRouteConfiguration configuration)
            => Use(ServiceDescriptor.Instance(instance, ServiceLifetime.Singleton), configuration);

        private LogRoute Use(IServiceDescriptor service, LogRouteConfiguration configuration)
        {
            Service = service;
            Configuration = configuration;

            return this;
        }
    }
}