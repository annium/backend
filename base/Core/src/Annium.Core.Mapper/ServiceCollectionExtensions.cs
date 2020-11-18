using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.DependencyInjection.Obsolete;
using Annium.Core.Mapper;
using Annium.Core.Mapper.Internal;
using Annium.Core.Mapper.Internal.DependencyInjection;
using Annium.Core.Reflection;
using Annium.Core.Runtime.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMapper(
            this IServiceCollection services,
            bool autoload = true
        )
        {
            // register base services
            services.TryAddSingleton<IRepacker, Repacker>();
            services.TryAddSingleton<IMapBuilder, MapBuilder>();
            services.TryAddSingleton<IMapper, Mapper.Internal.Mapper>();

            // register resolvers
            services.AddAllTypes(typeof(IMapResolver).Assembly, false)
                .AssignableTo<IMapResolver>()
                .As<IMapResolver>()
                .SingleInstance();

            // add default profile
            services.AddProfileInstance(new EmptyProfile());
            services.AddProfileInstance(new DefaultProfile());

            // if autoload requested - discover and register profiles
            if (autoload)
            {
                var typeManager = services.GetTypeManager();

                foreach (var profileType in typeManager.GetImplementations(typeof(Profile)))
                    services.AddProfileType(profileType);
            }

            // register profile resolution
            services.AddSingleton(ResolveProfiles);

            return services;
        }

        public static IServiceCollection AddProfile(
            this IServiceCollection services,
            Action<Profile> configure
        )
        {
            var profile = new EmptyProfile();
            configure(profile);

            services.AddSingleton(new ProfileInstance(profile));

            return services;
        }

        public static IServiceCollection AddProfile<T>(
            this IServiceCollection services
        )
            where T : Profile
        {
            services.AddProfileType(typeof(T));

            return services;
        }

        public static IServiceCollection AddProfile(
            this IServiceCollection services,
            Type profileType
        )
        {
            if (!profileType.GetInheritanceChain().Contains(typeof(Profile)))
                throw new ArgumentException($"Type {profileType} is not inherited from {typeof(Profile)}");

            services.AddProfileType(profileType);

            return services;
        }

        private static IServiceCollection AddProfileInstance<T>(
            this IServiceCollection services,
            T profile
        )
            where T : Profile
        {
            services.AddSingleton(profile);
            services.AddSingleton(new ProfileInstance(profile));

            return services;
        }

        private static IServiceCollection AddProfileType(
            this IServiceCollection services,
            Type profileType
        )
        {
            services.AddSingleton(profileType);
            services.AddSingleton(new ProfileType(profileType));

            return services;
        }

        private static IEnumerable<Profile> ResolveProfiles(
            IServiceProvider sp
        )
        {
            var baseInstances = sp.GetRequiredService<IEnumerable<ProfileInstance>>()
                .Select(x => x.Instance)
                .ToArray();

            var typeResolver = sp.GetRequiredService<ITypeResolver>();
            var types = sp.GetRequiredService<IEnumerable<ProfileType>>()
                .SelectMany(x => typeResolver.ResolveType(x.Type))
                .ToArray();
            var typeInstances = types
                .Select(sp.GetRequiredService)
                .OfType<Profile>()
                .ToArray();

            return baseInstances.Concat(typeInstances).ToArray();
        }
    }
}