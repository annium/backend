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

            container.Add<IEnumerable<LogRoute>>(routes).AsSelf().Singleton();

            foreach (var route in routes)
            {
                if (route.Service != null)
                    container.Add(route.Service);
            }

            container.Add(typeof(Logger<>)).As(typeof(ILogger<>)).Scoped();
            container.Add<ILoggerFactory, LoggerFactory>().Scoped();
            container.Add<ILogRouter, LogRouter>().Scoped();
            container.AddProfile(p =>
            {
                p.Map<LogLevel, string>(x => x.ToString());
                p.Map<string, LogLevel>(x => x.ParseEnum<LogLevel>());
            });

            return container;
        }
    }
}