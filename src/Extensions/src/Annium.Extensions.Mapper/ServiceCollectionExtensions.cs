using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

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

            var mapBuilder = new MapBuilder(cfg);
            var mapper = new Mapper(mapBuilder);

            services.AddSingleton<IMapper>(mapper);

            return services;
        }
    }
}