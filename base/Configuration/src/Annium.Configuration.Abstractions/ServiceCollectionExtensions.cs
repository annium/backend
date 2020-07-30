using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Configuration.Abstractions;
using Annium.Core.Mapper;
using Annium.Core.Reflection;
using Annium.Core.Runtime.Types;
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
            var serviceProvider = services.Clone().AddMapper().BuildServiceProvider();
            var typeManager = serviceProvider.GetRequiredService<ITypeManager>();
            var mapper = serviceProvider.GetRequiredService<IMapper>();
            var builder = new ConfigurationBuilder(typeManager, mapper);
            configure(builder);
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