using System;
using Annium.Core.Mapper;
using Annium.Core.Mapper.Internal;
using Annium.Core.Reflection;
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
            var cfg = new EmptyMapperConfiguration();
            configure(cfg);

            services.AddSingleton<MapperConfiguration>(cfg);

            return services;
        }

        public static IServiceCollection AddMapper(this IServiceCollection services, bool autoload = false)
        {
            services.AddReflectionTools();

            services.AddSingleton<Repacker>();
            services.AddSingleton<MapBuilder>();
            services.AddSingleton<IMapper, MapperInstance>();

            services.AddSingleton<MapperConfiguration>(new DefaultConfiguration());

            if (autoload)
            {
                var cfgType = typeof(MapperConfiguration);
                var implementations = TypeManager.Instance.GetImplementations(cfgType);
                foreach (var type in implementations)
                    services.AddSingleton(cfgType, type);
            }

            return services;
        }
    }
}