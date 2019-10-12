using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.Mapper.Internal;

namespace Annium.Core.Mapper
{
    public abstract class Profile
    {
        internal IReadOnlyDictionary<ValueTuple<Type, Type>, IMapConfigurationBase> Configurations => configurations;

        private readonly Dictionary<ValueTuple<Type, Type>, IMapConfigurationBase> configurations =
            new Dictionary<ValueTuple<Type, Type>, IMapConfigurationBase>();

        internal static Profile Merge(params Profile[] configurations)
        {
            var result = new EmptyProfile();

            foreach (var (key, map) in configurations.SelectMany(c => c.configurations))
                result.configurations[key] = map;

            return result;
        }

        public Profile Map<S, D>(Func<S, D> map)
        {
            var cfg = CreateMap<S, D>();
            cfg.Type = map;

            return this;
        }

        public IMapConfiguration<S, D> Map<S, D>()
        {
            var cfg = new MapConfiguration<S, D>();

            configurations[(typeof(S), typeof(D))] = cfg;

            return cfg;
        }

        public IMapConfiguration<S, D> MapGeneric<S, D>()
        {
            var cfg = new MapConfiguration<S, D>();

            configurations[(typeof(S), typeof(D))] = cfg;

            return cfg;
        }

        private MapConfiguration<S, D> CreateMap<S, D>()
        {
            var cfg = new MapConfiguration<S, D>();

            configurations[(typeof(S), typeof(D))] = cfg;

            return cfg;
        }

        // TODO: separate flow for generic types
    }
}
