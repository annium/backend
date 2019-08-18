using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMediatorConfiguration(
            this IServiceCollection services,
            Action<MediatorConfiguration> configure
        )
        {
            var cfg = new MediatorConfiguration();
            configure(cfg);

            services.AddSingleton<MediatorConfiguration>(cfg);

            return services;
        }

        public static IServiceCollection AddMediator(this IServiceCollection services, IServiceProvider provider = null)
        {
            var configurations = services.BuildServiceProvider().GetRequiredService<IEnumerable<MediatorConfiguration>>();
            if (provider != null)
                configurations = configurations.Union(provider.GetRequiredService<IEnumerable<MediatorConfiguration>>());

            var cfg = MediatorConfiguration.Merge(configurations.ToArray());

            services.AddSingleton(cfg);
            foreach (var handler in cfg.Handlers)
                services.AddScoped(handler.Implementation);

            services.AddScoped<IMediator, Mediator.Internal.Mediator>();

            return services;
        }
    }
}