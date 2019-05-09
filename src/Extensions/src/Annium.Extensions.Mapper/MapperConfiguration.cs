using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Annium.Extensions.Mapper
{
    public class MapperConfiguration
    {
        internal static MapperConfiguration Merge(params MapperConfiguration[] configurations)
        {
            var result = new MapperConfiguration();
            foreach (var(key, map) in configurations.SelectMany(c => c.maps))
                result.maps[key] = map;

            return result;
        }

        internal IReadOnlyDictionary<ValueTuple<Type, Type>, Map> Maps => maps;

        private Dictionary<ValueTuple<Type, Type>, Map> maps =
            new Dictionary<ValueTuple<Type, Type>, Map>();

        public MapperConfiguration Map<TSource, TTarget>(Expression<Func<TSource, TTarget>> map)
        {
            var cfg = new Map();
            cfg.TypeMap = map;

            return SaveMap(typeof(TSource), typeof(TTarget), cfg);
        }

        private MapperConfiguration SaveMap(Type src, Type tgt, Map cfg)
        {
            maps[(src, tgt)] = cfg;

            return this;
        }
    }
}