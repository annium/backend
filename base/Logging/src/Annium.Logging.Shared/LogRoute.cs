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
            where T : class, ILogHandler
            => Use(ServiceDescriptor.Type<T, T>(ServiceLifetime.Singleton), configuration);

        public LogRoute UseInstance<T>(T instance, LogRouteConfiguration configuration)
            where T : class, ILogHandler
            => Use(ServiceDescriptor.Instance(instance, ServiceLifetime.Singleton), configuration);

        public LogRoute UseFactory<T>(Func<IServiceProvider, T> factory, LogRouteConfiguration configuration)
            where T : class, ILogHandler
            => Use(ServiceDescriptor.Factory(factory, ServiceLifetime.Singleton), configuration);

        public LogRoute UseAsyncType<T>(LogRouteConfiguration configuration)
            where T : class, IAsyncLogHandler
            => Use(ServiceDescriptor.Type<T, T>(ServiceLifetime.Singleton), configuration);

        public LogRoute UseAsyncInstance<T>(T instance, LogRouteConfiguration configuration)
            where T : class, IAsyncLogHandler
            => Use(ServiceDescriptor.Instance(instance, ServiceLifetime.Singleton), configuration);

        public LogRoute UseAsyncFactory<T>(Func<IServiceProvider,T> factory, LogRouteConfiguration configuration)
            where T : class, IAsyncLogHandler
            => Use(ServiceDescriptor.Factory(factory, ServiceLifetime.Singleton), configuration);

        private LogRoute Use(IServiceDescriptor service, LogRouteConfiguration configuration)
        {
            Service = service;
            Configuration = configuration;

            return this;
        }
    }
}