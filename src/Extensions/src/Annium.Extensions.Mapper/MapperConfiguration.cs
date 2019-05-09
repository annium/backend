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
            var cfg = Map<TSource, TTarget>();

            cfg.Type(map);

            return this;
        }

        public MapConfiguration<TSource, TTarget> Map<TSource, TTarget>()
        {
            var cfg = new MapConfiguration<TSource, TTarget>();

            maps[(typeof(TSource), typeof(TTarget))] = cfg;

            return cfg;
        }
    }
}