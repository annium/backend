using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLogging(
            this IServiceCollection services,
            Action<LogRoute> configure
        )
        {
            var routes = new List<LogRoute>();
            configure(new LogRoute(routes.Add));
            routes = routes.Where(r => r.Service != null).ToList();

            services.AddSingleton<IEnumerable<LogRoute>>(routes);

            foreach (var route in routes)
                services.Add(route.Service);

            services.AddScoped(typeof(ILogger<>), typeof(Logger<>));
            services.AddScoped<ILogRouter, LogRouter>();

            return services;
        }
    }
}