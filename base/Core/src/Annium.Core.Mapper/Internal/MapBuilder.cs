using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Annium.Core.Mapper.Internal
{
    internal class MapBuilder : IMapBuilder
    {
        private readonly IEnumerable<IConfigurableMapResolver> _configurableMapResolvers;
        private readonly IEnumerable<IMapResolver> _mapResolvers;
        private readonly Profile _profile;
        private readonly IDictionary<ValueTuple<Type, Type>, Delegate> _maps = new Dictionary<ValueTuple<Type, Type>, Delegate>();
        private readonly IDictionary<ValueTuple<Type, Type>, Mapping> _mappings = new Dictionary<ValueTuple<Type, Type>, Mapping>();

        public MapBuilder(
            IEnumerable<Profile> configs,
            IEnumerable<IConfigurableMapResolver> configurableMapResolvers,
            IEnumerable<IMapResolver> mapResolvers,
            IRepacker repacker
        )
        {
            _configurableMapResolvers = configurableMapResolvers;
            _mapResolvers = mapResolvers;
            _profile = Profile.Merge(configs.ToArray());

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
            var mapping = ResolveMapping(src, tgt);

            var result = Expression.Lambda(mapping(param), param);
            // var str = result.ToReadableString();

            return _maps[key] = result.Compile();
        }

        private Mapping ResolveMapping(Type src, Type tgt)
        {
            if (src == tgt)
                return ex => ex;

            var key = (src, tgt);
            if (_mappings.TryGetValue(key, out var mapping))
                return mapping;

            var map = _profile.Maps.TryGetValue(key, out var cfg)
                ? BuildMapping(src, tgt, cfg)
                : BuildMapping(src, tgt);

            return _mappings[key] = map;
        }

        private Mapping BuildMapping(Type src, Type tgt, Map cfg)
        {
            var configurableMapResolver = _configurableMapResolvers.FirstOrDefault(x => x.CanResolveMap(src, tgt));
            if (configurableMapResolver != null)
                return configurableMapResolver.ResolveMap(src, tgt, cfg, ResolveMapping);

            return BuildMapping(src, tgt);
        }

        private Mapping BuildMapping(Type src, Type tgt)
        {
            var mapResolver = _mapResolvers.FirstOrDefault(x => x.CanResolveMap(src, tgt));
            if (mapResolver != null)
                return mapResolver.ResolveMap(src, tgt, ResolveMapping);

            throw new MappingException(src, tgt, "No map found.");
        }
    }
}