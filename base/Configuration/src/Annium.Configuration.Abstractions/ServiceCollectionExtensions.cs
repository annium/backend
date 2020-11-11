using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Annium.Configuration.Abstractions;
using Annium.Configuration.Abstractions.Internal;
using Annium.Core.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddConfiguration<T>(
            this IServiceCollection services,
            Action<IConfigurationContainer> configure
        )
            where T : class, new()
        {
            services.AddConfigurationBuilder();

            var container = new ConfigurationContainer();
            configure(container);

            services.AddSingleton(sp =>
            {
                var builder = sp.GetRequiredService<IConfigurationBuilder>();
                builder.Add(container.Get());

                return builder.Build<T>();
            });

            Register(services, typeof(T));

            return services;
        }

        public static async Task<IServiceCollection> AddConfiguration<T>(
            this IServiceCollection services,
            Func<IConfigurationContainer, Task> configure
        )
            where T : class, new()
        {
            services.AddConfigurationBuilder();

            var container = new ConfigurationContainer();
            await configure(container);

            services.AddSingleton(sp =>
            {
                var builder = sp.GetRequiredService<IConfigurationBuilder>();
                builder.Add(container.Get());

                return builder.Build<T>();
            });

            Register(services, typeof(T));

            return services;
        }

        private static void AddConfigurationBuilder(
            this IServiceCollection services
        )
        {
            services.TryAddTransient<IConfigurationBuilder, ConfigurationBuilder>();
            services.TryAddTransient<Func<IConfigurationBuilder>>(sp => sp.GetRequiredService<IConfigurationBuilder>);
        }

        private static void Register(
            IServiceCollection services,
            Type type
        )
        {
            foreach (var property in GetRegisteredProperties(type))
                Register(services, type, property);
        }


        private static void Register(
            IServiceCollection services,
            Type type,
            PropertyInfo property
        )
        {
            var propertyType = property.PropertyType;
            services.AddSingleton(propertyType, sp => property.GetValue(sp.GetRequiredService(type))!);

            foreach (var prop in GetRegisteredProperties(propertyType))
                Register(services, propertyType, prop);
        }

        private static IReadOnlyCollection<PropertyInfo> GetRegisteredProperties(Type type) => type
            .GetProperties()
            .Where(x =>
                x.CanRead &&
                !x.PropertyType.IsEnum &&
                !x.PropertyType.IsValueType &&
                !x.PropertyType.IsPrimitive &&
                !x.PropertyType.IsDerivedFrom(typeof(IEnumerable<>))
            )
            .ToArray();
    }
}