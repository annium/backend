using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.Application.Types;
using Annium.Core.Mapper;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMapperConfiguration(
            this IServiceCollection services,
            Action<MapperConfiguration> configure
        )
        {
            var cfg = new MapperConfiguration();
            configure(cfg);

            services.AddSingleton<MapperConfiguration>(cfg);

            return services;
        }

        public static IServiceCollection AddMapper(this IServiceCollection services, IServiceProvider provider = null)
        {
            var configurations = services.BuildServiceProvider().GetRequiredService<IEnumerable<MapperConfiguration>>();
            if (provider != null)
                configurations = configurations.Union(provider.GetRequiredService<IEnumerable<MapperConfiguration>>());

            var cfg = MapperConfiguration.Merge(configurations.ToArray());

            DefaultConfiguration.Apply(cfg);

            var repacker = new Repacker();
            var mapBuilder = new MapBuilder(cfg, TypeManager.Instance, repacker);
            var mapper = new MapperInstance(mapBuilder);

            services.AddSingleton<IMapper>(mapper);

            return services;
        }
    }
}