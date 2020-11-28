using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.Mapper;
using Annium.Core.Mapper.Internal;
using Annium.Core.Mapper.Internal.DependencyInjection;
using Annium.Core.Reflection;
using Annium.Core.Runtime.Types;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceContainerExtensions
    {
        public static IServiceContainer AddMapper(
            this IServiceContainer container,
            bool autoload = true
        )
        {
            // register base services
            container.Add<IRepacker, Repacker>().Singleton();
            container.Add<IMapBuilder, MapBuilder>().Singleton();
            container.Add<IMapper, Mapper.Internal.Mapper>().Singleton();

            // register resolvers
            container.AddAll(typeof(IMapResolver).Assembly, false)
                .AssignableTo<IMapResolver>()
                .As<IMapResolver>()
                .Singleton();

            // add default profile
            container.AddProfileInstance(new EmptyProfile());
            container.AddProfileInstance(new DefaultProfile());

            // if autoload requested - discover and register profiles
            if (autoload)
            {
                var typeManager = container.GetTypeManager();

                foreach (var profileType in typeManager.GetImplementations(typeof(Profile)))
                    container.AddProfileType(profileType);
            }

            // register profile resolution
            container.Add(ResolveProfiles).AsSelf().Singleton();

            return container;
        }

        public static IServiceContainer AddProfile(
            this IServiceContainer container,
            Action<Profile> configure
        )
        {
            var profile = new EmptyProfile();
            configure(profile);

            container.Add(new ProfileInstance(profile)).AsSelf().Singleton();

            return container;
        }

        public static IServiceContainer AddProfile<T>(
            this IServiceContainer container
        )
            where T : Profile
        {
            container.AddProfileType(typeof(T));

            return container;
        }

        public static IServiceContainer AddProfile(
            this IServiceContainer container,
            Type profileType
        )
        {
            if (!profileType.GetInheritanceChain().Contains(typeof(Profile)))
                throw new ArgumentException($"Type {profileType} is not inherited from {typeof(Profile)}");

            container.AddProfileType(profileType);

            return container;
        }

        private static IServiceContainer AddProfileInstance<T>(
            this IServiceContainer container,
            T profile
        )
            where T : Profile
        {
            container.Add(profile).AsSelf().Singleton();
            container.Add(new ProfileInstance(profile)).AsSelf().Singleton();

            return container;
        }

        private static IServiceContainer AddProfileType(
            this IServiceContainer container,
            Type profileType
        )
        {
            container.Add(profileType).AsSelf().Singleton();
            container.Add(new ProfileType(profileType)).AsSelf().Singleton();

            return container;
        }

        private static IEnumerable<Profile> ResolveProfiles(
            IServiceProvider sp
        )
        {
            var baseInstances = sp.Resolve<IEnumerable<ProfileInstance>>()
                .Select(x => x.Instance)
                .ToArray();

            var typeResolver = sp.Resolve<ITypeResolver>();
            var types = sp.Resolve<IEnumerable<ProfileType>>()
                .SelectMany(x => typeResolver.ResolveType(x.Type))
                .ToArray();
            var typeInstances = types
                .Select(sp.Resolve)
                .OfType<Profile>()
                .ToArray();

            return baseInstances.Concat(typeInstances).ToArray();
        }
    }
}