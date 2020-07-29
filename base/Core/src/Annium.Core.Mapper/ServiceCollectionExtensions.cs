using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.Mapper;
using Annium.Core.Mapper.Internal;
using Annium.Core.Reflection;
using Annium.Core.Runtime.Types;
using Microsoft.Extensions.DependencyInjection;

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
            {
                var typeManager = services.BuildServiceProvider().GetRequiredService<ITypeManager>();
                foreach (var profile in typeManager.GetImplementations(typeof(Profile)))
                    services.AddProfile(profile);
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
            => services.AddProfileInternal(typeof(T));

        public static IServiceCollection AddProfile(
            this IServiceCollection services,
            Type profileType
        )
        {
            if (!profileType.GetInheritanceChain().Contains(typeof(Profile)))
                throw new ArgumentException($"Type {profileType} is not inherited from {typeof(Profile)}");

            return services.AddProfileInternal(profileType);
        }

        private static IServiceCollection AddProfileInternal(
            this IServiceCollection services,
            Type profileType
        )
        {
            if (profileType.IsGenericType)
            {
                var typeManager = services.BuildServiceProvider().GetRequiredService<ITypeManager>();
                var sets = new List<Type[]>();
                foreach (var argument in profileType.GetGenericArguments())
                {
                    if (argument.GetGenericParameterConstraints().Length == 0)
                        throw new ArgumentException(
                            $"Can't add generic Profile {profileType} with unconstrained parameter {argument}");
                    var implementations = typeManager.Types
                        .Where(x => !x.ContainsGenericParameters)
                        .Select(x => x.GetTargetImplementation(argument))
                        .Where(x => x != null)
                        .ToArray();
                    sets.Add(implementations!);
                }

                var combinations = GetCombinations(sets).ToArray();
                foreach (var combination in combinations)
                {
                    var concreteType = profileType.MakeGenericType(combination.ToArray());
                    services.AddSingleton(typeof(Profile), concreteType);
                }
            }
            else
                services.AddSingleton(typeof(Profile), profileType);

            return services;

            static IEnumerable<IEnumerable<Type>> GetCombinations(IEnumerable<Type[]> sets)
            {
                if (sets.Count() == 1)
                    return sets.ElementAt(0).Select(x => new[] { x });

                return sets.ElementAt(0).SelectMany(x => GetCombinations(sets.Skip(1)).Select(y => y.Prepend(x)));
            }
        }
    }
}