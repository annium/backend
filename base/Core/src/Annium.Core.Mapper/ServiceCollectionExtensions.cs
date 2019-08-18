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

        public static IServiceCollection AddMapper(this IServiceCollection services)
        {
            var cfg = MapperConfiguration.Merge(services.BuildServiceProvider().GetRequiredService<IEnumerable<MapperConfiguration>>().ToArray());

            DefaultConfiguration.Apply(cfg);

            var repacker = new Repacker();
            var mapBuilder = new MapBuilder(cfg, TypeManager.Instance, repacker);
            var mapper = new MapperInstance(mapBuilder);

            services.AddSingleton<IMapper>(mapper);

            return services;
        }
    }
}