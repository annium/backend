using System;
using Annium.Core.Mapper;
using Annium.Core.Mapper.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddProfile(
            this IServiceCollection services,
            Action<Profile> configure
        )
        {
            var profile = new EmptyProfile();
            configure(profile);

            services.AddSingleton<Profile>(profile);

            return services;
        }

        public static IServiceCollection AddMapper(this IServiceCollection services, bool autoload = true)
        {
            services.AddRuntimeTools();

            // register base services
            services.AddSingleton<IRepacker, Repacker>();
            services.AddSingleton<IMapBuilder, MapBuilder>();
            services.AddSingleton<IMapper, MapperInstance>();

            // register resolvers
            services.AddAllTypes()
                .AssignableTo<IMapResolver>()
                .As<IMapResolver>()
                .SingleInstance();

            // add default profile
            services.AddSingleton<Profile>(new DefaultProfile());

            // if autoload requested - discover and register profiles
            if (autoload)
                services.AddAllTypes()
                    .AssignableTo<Profile>()
                    .As<Profile>()
                    .SingleInstance();

            return services;
        }
    }
}