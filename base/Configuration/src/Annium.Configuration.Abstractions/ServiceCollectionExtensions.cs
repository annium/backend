using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Configuration.Abstractions;
using Annium.Configuration.Abstractions.Internal;
using Annium.Core.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddConfigurationBuilder(
            this IServiceCollection services
        )
        {
            services.AddTransient<IConfigurationBuilder, ConfigurationBuilder>();
            services.AddTransient<Func<IConfigurationBuilder>>(sp => sp.GetRequiredService<IConfigurationBuilder>);

            return services;
        }

        public static IServiceCollection AddConfiguration<T>(
            this IServiceCollection services,
            Action<IConfigurationBuilder> configure
        )
            where T : class, new()
        {
            var serviceProvider = services.Clone().AddMapper().AddConfigurationBuilder().BuildServiceProvider();
            var builder = serviceProvider.GetRequiredService<IConfigurationBuilder>();
            configure(builder);
            var configuration = builder.Build<T>();

            Register(services, configuration);

            return services;
        }

        public static async Task<IServiceCollection> AddConfigurationAsync<T>(
            this IServiceCollection services,
            Func<IConfigurationBuilder, Task> configure
        )
            where T : class, new()
        {
            var serviceProvider = services.Clone().AddMapper().AddConfigurationBuilder().BuildServiceProvider();
            var builder = serviceProvider.GetRequiredService<IConfigurationBuilder>();
            await configure(builder);
            var configuration = builder.Build<T>();

            Register(services, configuration);

            return services;
        }

        private static void Register(IServiceCollection services, object cfg)
        {
            services.AddSingleton(cfg.GetType(), cfg);

            var props = cfg.GetType().GetProperties()
                .Where(x =>
                    x.CanRead &&
                    !x.PropertyType.IsEnum &&
                    !x.PropertyType.IsValueType &&
                    !x.PropertyType.IsPrimitive &&
                    !x.PropertyType.IsDerivedFrom(typeof(IEnumerable<>))
                )
                .ToList();

            foreach (var prop in props)
                Register(services, prop.GetValue(cfg)!);
        }
    }
}