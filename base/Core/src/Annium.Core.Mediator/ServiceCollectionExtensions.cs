using System;
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
            foreach (var handler in cfg.Handlers)
                services.AddScoped(handler.Implementation);

            return services;
        }

        public static IServiceCollection AddMediator(this IServiceCollection services)
        {
            services.AddSingleton<Mediator.Internal.ChainBuilder>();
            services.AddSingleton<Mediator.Internal.NextBuilder>();
            services.AddSingleton<IMediator, Mediator.Internal.Mediator>();

            return services;
        }
    }
}