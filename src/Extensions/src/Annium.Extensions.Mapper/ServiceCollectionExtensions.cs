using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Extensions.Mapper;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Extensions.DependencyInjection
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

        public static IServiceCollection AddMapper(this IServiceCollection services, IServiceProvider provider = null, bool statically = true)
        {
            var cfg = provider == null ?
                new MapperConfiguration() :
                MapperConfiguration.Merge(provider.GetRequiredService<IEnumerable<MapperConfiguration>>().ToArray());

            DefaultConfiguration.Apply(cfg);

            var typeResolver = statically ? TypeResolver.Instance : new TypeResolver();
            var repacker = new Repacker();
            var mapBuilder = new MapBuilder(cfg, typeResolver, repacker);
            var mapper = new MapperInstance(mapBuilder);

            services.AddSingleton<IMapper>(mapper);

            return services;
        }
    }
}