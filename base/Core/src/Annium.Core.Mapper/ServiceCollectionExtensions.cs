using System;
using Annium.Core.Mapper;
using Annium.Core.Mapper.Internal;
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
            services.AddReflectionTools();

            services.AddMapperConfiguration(DefaultConfiguration.Apply);

            services.AddSingleton<Repacker>();
            services.AddSingleton<MapBuilder>();
            services.AddSingleton<IMapper, MapperInstance>();

            return services;
        }
    }
}