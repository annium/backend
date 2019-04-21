using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Extensions.Mapper
{
    public static class OldServiceCollectionExtensions
    {
        public static IServiceCollection AddMapperConfigurationOld(
            this IServiceCollection services,
            Func<AutoMapper.Configuration.MapperConfigurationExpression> configure
        )
        {
            services.AddSingleton<AutoMapper.Configuration.MapperConfigurationExpression>(configure());

            return services;
        }

        public static void AddMapperOld(this IServiceCollection services, IServiceProvider provider)
        {
            var cfg = new AutoMapper.Configuration.MapperConfigurationExpression();
            foreach (var profile in provider.GetRequiredService<IEnumerable<AutoMapper.Configuration.MapperConfigurationExpression>>())
                cfg.AddProfile(profile);

            var mapperConfiguration = new AutoMapper.MapperConfiguration(cfg);

            mapperConfiguration.AssertConfigurationIsValid();

            services.AddSingleton<AutoMapper.IMapper>(mapperConfiguration.CreateMapper());
        }
    }
}