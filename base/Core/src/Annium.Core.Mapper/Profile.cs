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
            foreach (var (key, map) in configurations.SelectMany(c => c._maps))
                result._maps[key] = map;

            return result;
        }

        internal IReadOnlyDictionary<ValueTuple<Type, Type>, Map> Maps => _maps;

        private readonly Dictionary<ValueTuple<Type, Type>, Map> _maps =
            new Dictionary<ValueTuple<Type, Type>, Map>();

        public void Map<TSource, TTarget>(Expression<Func<TSource, TTarget>> mapping)
        {
            var map = Map<TSource, TTarget>();

            map.Type(mapping);
        }

        public Map<TSource, TTarget> Map<TSource, TTarget>()
        {
            var map = new Map<TSource, TTarget>();

            _maps[(typeof(TSource), typeof(TTarget))] = map;

            return map;
        }
    }
}