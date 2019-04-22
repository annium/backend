using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace Annium.Extensions.Mapper
{
    public static class ServiceCollectionExtensions
    {
        private static Lazy<TypeResolver> TypeResolver = new Lazy<TypeResolver>(() => new TypeResolver(), true);

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

        public static IServiceCollection AddMapper(this IServiceCollection services, IServiceProvider provider, bool statically = false)
        {
            var cfg = MapperConfiguration
                .Merge(provider.GetRequiredService<IEnumerable<MapperConfiguration>>().ToArray());

            ConfigureMapping(cfg);

            var typeResolver = statically ? TypeResolver.Value : new TypeResolver();
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