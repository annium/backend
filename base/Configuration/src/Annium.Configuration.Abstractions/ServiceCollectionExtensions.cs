using System;
using System.Linq;
using Annium.Configuration.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddConfiguration<T>(
            this IServiceCollection services,
            Action<IConfigurationBuilder> configure
        )
        where T : class, new()
        {
            var builder = new ConfigurationBuilder();
            configure(builder);
            var configuration = builder.Build<T>();

            Register(services, configuration);

            return services;
        }

        private static void Register(IServiceCollection services, object cfg)
        {
            services.AddSingleton(cfg);

            var props = cfg.GetType().GetProperties()
                .Where(p => p.CanRead && !p.PropertyType.IsEnum && !p.PropertyType.IsValueType && !p.PropertyType.IsPrimitive)
                .ToList();

            foreach (var prop in props)
                Register(services, prop.GetValue(cfg)!);
        }
    }
}