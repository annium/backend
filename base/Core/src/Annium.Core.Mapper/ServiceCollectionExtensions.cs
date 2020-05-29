using System;
using Annium.Core.Mapper;
using Annium.Core.Mapper.Internal;
using Annium.Core.Runtime.Types;
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
            services.SelectTypes()
                .Where(x => typeof(IMapResolver).IsAssignableFrom(x))
                .AsImplementedInterfaces()
                .RegisterSingleton();

            // add default profile
            services.AddSingleton<Profile>(new DefaultProfile());

            // if autoload requested - discover and register profiles
            if (autoload)
            {
                var profileBase = typeof(Profile);
                var profiles = TypeManager.Instance.GetImplementations(profileBase);
                foreach (var profile in profiles)
                    services.AddSingleton(profileBase, profile);
            }

            return services;
        }
    }
}