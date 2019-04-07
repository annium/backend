using System;
using System.Collections.Generic;
using AutoMapper;
using AutoMapper.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Extensions.Mapper
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMapperConfiguration(
            this IServiceCollection services,
            Func<MapperConfigurationExpression> configure
        )
        {
            services.AddSingleton<MapperConfigurationExpression>(configure());

            return services;
        }

        public static void AddMapper(this IServiceCollection services, IServiceProvider provider)
        {
            var cfg = new MapperConfigurationExpression();
            foreach (var profile in provider.GetRequiredService<IEnumerable<MapperConfigurationExpression>>())
                cfg.AddProfile(profile);

            var mapperConfiguration = new MapperConfiguration(cfg);

            mapperConfiguration.AssertConfigurationIsValid();

            services.AddSingleton<IMapper>(mapperConfiguration.CreateMapper());
        }
    }
}