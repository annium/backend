using System;
using Annium.Core.Mediator;
using Annium.Core.Runtime.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMediatorConfiguration(
            this IServiceCollection services,
            Action<MediatorConfiguration, ITypeManager> configure
        )
        {
            var cfg = new MediatorConfiguration();
            var typeManager = services.BuildServiceProvider().GetRequiredService<ITypeManager>();
            configure(cfg, typeManager);

            return services.AddMediatorConfiguration(cfg);
        }

        public static IServiceCollection AddMediatorConfiguration(
            this IServiceCollection services,
            Action<MediatorConfiguration> configure
        )
        {
            var cfg = new MediatorConfiguration();
            configure(cfg);

            return services.AddMediatorConfiguration(cfg);
        }

        public static IServiceCollection AddMediator(this IServiceCollection services)
        {
            services.AddSingleton<Mediator.Internal.ChainBuilder>();
            services.AddSingleton<Mediator.Internal.NextBuilder>();
            services.AddSingleton<IMediator, Mediator.Internal.Mediator>();

            return services;
        }

        private static IServiceCollection AddMediatorConfiguration(
            this IServiceCollection services,
            MediatorConfiguration cfg
        )
        {
            services.AddSingleton(cfg);
            foreach (var handler in cfg.Handlers)
                services.AddScoped(handler.Implementation);

            return services;
        }
    }
}