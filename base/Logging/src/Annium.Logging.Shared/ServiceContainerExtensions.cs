using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.Primitives;
using Annium.Logging.Abstractions;
using Annium.Logging.Shared;
using Annium.Logging.Shared.Internal;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceContainerExtensions
    {
        public static IServiceContainer AddLogging<TContext>(
            this IServiceContainer container,
            Action<LogRoute<TContext>> configure
        )
            where TContext : class, ILogContext
        {
            return container.AddLoggingBase(configure, ServiceLifetime.Scoped);
        }

        public static IServiceContainer AddLogging(
            this IServiceContainer container,
            Action<LogRoute<DefaultLogContext>> configure
        )
        {
            return container.AddLoggingBase(configure, ServiceLifetime.Singleton);
        }

        private static IServiceContainer AddLoggingBase<TContext>(
            this IServiceContainer container,
            Action<LogRoute<TContext>> configure,
            ServiceLifetime lifetime
        )
            where TContext : class, ILogContext
        {
            var routes = new List<LogRoute<TContext>>();
            configure(new LogRoute<TContext>(routes.Add));
            routes = routes.Where(r => r.Service != null).ToList();

            foreach (var route in routes)
            {
                var cfg = route.Configuration!;
                var serviceType = route.Service!.ServiceType;
                if (typeof(ILogHandler<TContext>).IsAssignableFrom(serviceType))
                    container.Add(sp => new ImmediateLogScheduler<TContext>(
                        route.Filter,
                        (ILogHandler<TContext>)sp.Resolve(route.Service!.ServiceType)
                    )).AsInterfaces().Singleton();
                else if (typeof(IAsyncLogHandler<TContext>).IsAssignableFrom(serviceType))
                    container.Add(sp => new BackgroundLogScheduler<TContext>(
                        route.Filter,
                        (IAsyncLogHandler<TContext>)sp.Resolve(route.Service!.ServiceType),
                        cfg
                    )).AsInterfaces().Singleton();
                else
                    throw new Exception($"Unsupported log handler service type: {serviceType.FriendlyName()}");
            }

            foreach (var route in routes)
            {
                if (route.Service != null)
                    container.Add(route.Service);
            }

            container.Add<TContext>().AsSelf().In(lifetime);
            container.Add(typeof(Logger<>)).As(typeof(ILogger<>)).In(lifetime);
            container.Add(typeof(LogSubject<>)).As(typeof(ILogSubject<>)).In(lifetime);
            container.Add<ILoggerFactory, LoggerFactory>().In(lifetime);
            container.Add<LogRouter<TContext>>().AsSelf().Singleton();
            container.Add<ILogSentry<TContext>, LogSentry<TContext>>().Singleton();
            container.Add<ILogSentryBridge, LogSentryBridge<TContext>>().In(lifetime);
            container.AddProfile(p =>
            {
                p.Map<LogLevel, string>(x => x.ToString());
                p.Map<string, LogLevel>(x => x.ParseEnum<LogLevel>());
            });

            container.OnBuild += sp => sp.Resolve<LogRouter<TContext>>();

            return container;
        }
    }
}