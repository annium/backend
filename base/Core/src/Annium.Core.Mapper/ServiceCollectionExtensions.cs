using System;
using Annium.Core.Mapper;
using Annium.Core.Reflection;
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
            services.AddReflectionTools();

            services.AddSingleton<Mapper.Internal.MapBuilder>();
            services.AddSingleton<IMapper, Mapper.Internal.Mapper>();

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