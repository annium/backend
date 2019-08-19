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

        public static IServiceCollection AddMediator(this IServiceCollection services)
        {
            var cfg = MediatorConfiguration.Merge(services.BuildServiceProvider().GetRequiredService<IEnumerable<MediatorConfiguration>>().ToArray());

            services.AddSingleton(cfg);
            foreach (var handler in cfg.Handlers)
                services.AddScoped(handler.Implementation);

            services.AddSingleton<Mediator.Internal.ChainBuilder>();
            services.AddSingleton<Mediator.Internal.NextBuilder>();
            services.AddScoped<IMediator, Mediator.Internal.Mediator>();

            return services;
        }
    }
}