using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Annium.Core.Reflection;

namespace Annium.Core.Mapper.Internal
{
    internal partial class MapBuilder
    {
        private readonly IDictionary<ValueTuple<Type, Type>, Delegate> maps =
            new Dictionary<ValueTuple<Type, Type>, Delegate>();

        private readonly IDictionary<ValueTuple<Type, Type>, Func<Expression, Expression>> mappings =
            new Dictionary<ValueTuple<Type, Type>, Func<Expression, Expression>>();

        private readonly Profile profile;

        private readonly ITypeManager typeManager;

        private readonly Repacker repacker;

        public MapBuilder(
            IEnumerable<Profile> configs,
            ITypeManager typeManager,
            Repacker repacker
        )
        {
            profile = Profile.Merge(configs.ToArray());
            this.typeManager = typeManager;
            this.repacker = repacker;

            // save complete type maps directly to raw resolutions
            foreach (((Type, Type) key, Map map) in profile.Maps)
                if (map.Type != null)
                    mappings[key] = repacker.Repack(map.Type.Body);
        }

        public bool HasMap(Type src, Type tgt) => src == tgt || mappings.ContainsKey((src, tgt));

        public Delegate GetMap(Type src, Type tgt)
        {
            var key = (src, tgt);
            if (maps.TryGetValue(key, out var map))
                return map;

            var param = Expression.Parameter(src);
            var mapping = ResolveMap(src, tgt);
            if (mapping is null)
                throw new MappingException(src, tgt, $"No map found.");

            var result = Expression.Lambda(mapping(param), param);

            return maps[key] = result.Compile();
        }

        private Func<Expression, Expression> ResolveMap(Type src, Type tgt)
        {
            if (src == tgt)
                return null!;

            var key = (src, tgt);
            if (mappings.TryGetValue(key, out var mapping))
                return mapping;

            profile.Maps.TryGetValue(key, out var cfg);

            return mappings[key] = BuildMap(src, tgt, cfg!);
        }
    }
}