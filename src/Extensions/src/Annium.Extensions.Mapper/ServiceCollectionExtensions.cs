using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace Annium.Extensions.Mapper
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

        public static IServiceCollection AddMapper(this IServiceCollection services, IServiceProvider provider)
        {
            var cfg = MapperConfiguration
                .Merge(provider.GetRequiredService<IEnumerable<MapperConfiguration>>().ToArray());

            ConfigureMapping(cfg);

            var typeResolver = new TypeResolver();
            var repacker = new Repacker();
            var mapBuilder = new MapBuilder(cfg, typeResolver, repacker);
            var mapper = new Mapper(mapBuilder);

            services.AddSingleton<IMapper>(mapper);

            return services;
        }

        private static void ConfigureMapping(MapperConfiguration cfg)
        {
            cfg.Map<DateTime, Instant>(d => Instant.FromDateTimeUtc(d.ToUniversalTime()));
            cfg.Map<Instant, DateTime>(i => i.ToDateTimeUtc());
        }
    }
}