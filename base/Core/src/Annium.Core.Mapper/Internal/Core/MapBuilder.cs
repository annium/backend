using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Annium.Core.Reflection;

namespace Annium.Core.Mapper.Internal
{
    internal class MapBuilder
    {
        private readonly IDictionary<ValueTuple<Type, Type>, Delegate> maps =
            new Dictionary<ValueTuple<Type, Type>, Delegate>();
        private readonly ITypeManager typeManager;

        public MapBuilder(
            IEnumerable<Profile> profiles,
            ITypeManager typeManager
        )
        {
            this.typeManager = typeManager;

            var profile = Profile.Merge(profiles.ToArray());

            // save complete type maps directly to raw resolutions
            foreach (((Type, Type) key, IMapConfigurationBase cfg) in profile.Configurations)
                if (cfg.Type != null)
                    maps[key] = cfg.Type;
        }

        public bool HasMap(Type src, Type tgt) => src == tgt || maps.ContainsKey((src, tgt));

        public Delegate GetMap(Type src, Type tgt)
        {
            var key = (src, tgt);
            if (maps.TryGetValue(key, out var map))
                return map;

            return maps[key] = BuildMap(src, tgt)
                ?? throw new MappingException(src, tgt, $"No map found.");
        }

        private Delegate BuildMap(Type src, Type tgt)
        {
            // implement delegate building
            return null!;
        }
    }
}