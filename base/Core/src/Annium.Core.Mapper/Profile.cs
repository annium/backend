using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Annium.Core.Mapper
{
    public abstract class Profile
    {
        internal static Profile Merge(params Profile[] configurations)
        {
            var result = new EmptyProfile();
            foreach (var (key, map) in configurations.SelectMany(c => c.maps))
                result.maps[key] = map;

            return result;
        }

        internal IReadOnlyDictionary<ValueTuple<Type, Type>, Map> Maps => maps;

        private readonly Dictionary<ValueTuple<Type, Type>, Map> maps =
            new Dictionary<ValueTuple<Type, Type>, Map>();

        public Profile Map<TSource, TTarget>(Expression<Func<TSource, TTarget>> mapping)
        {
            var map = Map<TSource, TTarget>();

            map.Type(mapping);

            return this;
        }

        public Map<TSource, TTarget> Map<TSource, TTarget>()
        {
            var map = new Map<TSource, TTarget>();

            maps[(typeof(TSource), typeof(TTarget))] = map;

            return map;
        }
    }
}