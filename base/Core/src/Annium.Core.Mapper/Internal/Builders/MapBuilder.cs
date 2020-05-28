using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Annium.Core.Runtime.Types;

namespace Annium.Core.Mapper.Internal.Builders
{
    internal partial class MapBuilder
    {
        private readonly IDictionary<ValueTuple<Type, Type>, Delegate> _maps =
            new Dictionary<ValueTuple<Type, Type>, Delegate>();

        private readonly IDictionary<ValueTuple<Type, Type>, Mapping> _mappings =
            new Dictionary<ValueTuple<Type, Type>, Mapping>();

        private readonly Profile _profile;

        private readonly ITypeManager _typeManager;

        private readonly IRepacker _repacker;

        public MapBuilder(
            IEnumerable<Profile> configs,
            ITypeManager typeManager,
            IRepacker repacker
        )
        {
            _profile = Profile.Merge(configs.ToArray());
            _typeManager = typeManager;
            _repacker = repacker;

            // save complete type maps directly to raw resolutions
            foreach (((Type, Type) key, Map map) in _profile.Maps)
                if (map.Type != null)
                    _mappings[key] = repacker.Repack(map.Type.Body);
        }

        public bool HasMap(Type src, Type tgt) => src == tgt || _mappings.ContainsKey((src, tgt));

        public Delegate GetMap(Type src, Type tgt)
        {
            var key = (src, tgt);
            if (_maps.TryGetValue(key, out var map))
                return map;

            var param = Expression.Parameter(src);
            var mapping = ResolveMap(src, tgt);
            if (mapping is null)
                throw new MappingException(src, tgt, "No map found.");

            var result = Expression.Lambda(mapping(param), param);
            // var str = result.ToReadableString();

            return _maps[key] = result.Compile();
        }

        private Mapping? ResolveMap(Type src, Type tgt)
        {
            if (src == tgt)
                return null;

            var key = (src, tgt);
            if (_mappings.TryGetValue(key, out var mapping))
                return mapping;

            var map = _profile.Maps.TryGetValue(key, out var cfg)
                ? BuildMap(src, tgt, cfg)
                : BuildMap(src, tgt);

            return _mappings[key] = map;
        }
    }
}