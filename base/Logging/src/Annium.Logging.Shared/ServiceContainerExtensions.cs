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
        public static IServiceContainer AddLogging(
            this IServiceContainer container,
            Action<LogRoute> configure
        )
        {
            var routes = new List<LogRoute>();
            configure(new LogRoute(routes.Add));
            routes = routes.Where(r => r.Service != null).ToList();

            foreach (var route in routes)
            {
                var cfg = route.Configuration!;
                var serviceType = route.Service!.ServiceType;
                if (typeof(ILogHandler).IsAssignableFrom(serviceType))
                    container.Add(sp => new ImmediateLogScheduler(
                        route.Filter,
                        (ILogHandler) sp.Resolve(route.Service!.ServiceType)
                    )).AsInterfaces().Singleton();
                else if (typeof(IAsyncLogHandler).IsAssignableFrom(serviceType))
                    container.Add(sp => new BackgroundLogScheduler(
                        route.Filter,
                        (IAsyncLogHandler) sp.Resolve(route.Service!.ServiceType),
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

            container.Add(typeof(Logger<>)).As(typeof(ILogger<>)).Singleton();
            container.Add(typeof(LogSubject<>)).As(typeof(ILogSubject<>)).Singleton();
            container.Add<ILoggerFactory, LoggerFactory>().Singleton();
            container.Add<LogRouter>().AsSelf().Singleton();
            container.Add<ILogSentry, LogSentry>().AsSelf().Singleton();
            container.AddProfile(p =>
            {
                p.Map<LogLevel, string>(x => x.ToString());
                p.Map<string, LogLevel>(x => x.ParseEnum<LogLevel>());
            });

            container.OnBuild += sp => sp.Resolve<LogRouter>();

            return container;
        }
    }
}