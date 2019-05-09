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

        internal IReadOnlyDictionary<ValueTuple<Type, Type>, LambdaExpression> Maps => maps;

        private Dictionary<ValueTuple<Type, Type>, LambdaExpression> maps =
            new Dictionary<ValueTuple<Type, Type>, LambdaExpression>();

        public MapperConfiguration Map<TSource, TTarget>(Expression<Func<TSource, TTarget>> map)
        {
            maps[(typeof(TSource), typeof(TTarget))] = map;

            return this;
        }
    }
}