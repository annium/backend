using System;
using System.Linq;
using Annium.Core.Mapper;
using Annium.Core.Mapper.Internal;
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
            services.TryAddSingleton<Profile>(new DefaultProfile());

            // if autoload requested - discover and register profiles
            if (autoload)
            {
                var serviceProvider = services.BuildServiceProvider();
                var typeManager = serviceProvider.GetRequiredService<ITypeManager>();
                var typeResolver = serviceProvider.GetRequiredService<ITypeResolver>();

                foreach (var profile in typeManager.GetImplementations(typeof(Profile)))
                    services.AddProfileInternal(profile, typeResolver);
            }

            return services;
        }

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

        public static IServiceCollection AddProfile<T>(
            this IServiceCollection services
        )
            where T : Profile
        {
            services.AddSingleton(typeof(Profile), typeof(T));

            return services;
        }

        public static IServiceCollection AddProfile(
            this IServiceCollection services,
            Type profileType
        )
        {
            if (!profileType.GetInheritanceChain().Contains(typeof(Profile)))
                throw new ArgumentException($"Type {profileType} is not inherited from {typeof(Profile)}");

            var profileTypeResolver = services.BuildServiceProvider().GetRequiredService<ITypeResolver>();

            return services.AddProfileInternal(profileType, profileTypeResolver);
        }

        private static IServiceCollection AddProfileInternal(
            this IServiceCollection services,
            Type profileType,
            ITypeResolver typeResolver
        )
        {
            var types = typeResolver.ResolveType(profileType);

            foreach (var type in types)
                services.AddSingleton(typeof(Profile), type);

            return services;
        }
    }
}