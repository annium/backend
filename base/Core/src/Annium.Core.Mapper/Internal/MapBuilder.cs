using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AgileObjects.ReadableExpressions;

namespace Annium.Core.Mapper.Internal
{
    internal class MapBuilder : IMapBuilder
    {
        private readonly IEnumerable<IMapResolver> _mapResolvers;
        private readonly Profile _profile;
        private readonly IDictionary<ValueTuple<Type, Type>, Delegate> _maps = new Dictionary<ValueTuple<Type, Type>, Delegate>();
        private readonly IDictionary<ValueTuple<Type, Type>, Mapping> _mappings = new Dictionary<ValueTuple<Type, Type>, Mapping>();
        private readonly IMappingContext _context;

        public MapBuilder(
            IEnumerable<Profile> profiles,
            IEnumerable<IMapResolver> mapResolvers,
            IRepacker repacker
        )
        {
            _mapResolvers = mapResolvers;
            _profile = Profile.Merge(profiles.ToArray());

            // save complete type maps directly to raw resolutions
            foreach (((Type, Type) key, IMapConfiguration cfg) in _profile.MapConfigurations)
                if (cfg.MapWith != null)
                    _mappings[key] = repacker.Repack(cfg.MapWith.Body);

            _context = new MappingContext(GetMap, ResolveMapping);
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
            var str = result.ToReadableString();

            lock (_maps)
                return _maps[key] = result.Compile();
        }

        private Mapping ResolveMapping(Type src, Type tgt)
        {
            if (src == tgt)
                return ex => ex;

            var key = (src, tgt);
            if (_mappings.TryGetValue(key, out var mapping))
                return mapping;

            var map = _profile.MapConfigurations.TryGetValue(key, out var cfg)
                ? BuildMapping(src, tgt, cfg)
                : BuildMapping(src, tgt, MapConfiguration.Empty);

            return _mappings[key] = map;
        }

        private Mapping BuildMapping(Type src, Type tgt, IMapConfiguration cfg)
        {
            var mapResolver = _mapResolvers.OrderBy(x => x.Order).FirstOrDefault(x => x.CanResolveMap(src, tgt));
            if (mapResolver != null)
                return mapResolver.ResolveMap(src, tgt, cfg, _context);

            throw new MappingException(src, tgt, "No map found.");
        }
    }
}