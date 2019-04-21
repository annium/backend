using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Annium.Extensions.Mapper
{
    public partial class MapBuilder
    {
        private readonly IDictionary<ValueTuple<Type, Type>, Delegate> maps =
            new Dictionary<ValueTuple<Type, Type>, Delegate>();

        private readonly IDictionary<ValueTuple<Type, Type>, LambdaExpression> raw =
            new Dictionary<ValueTuple<Type, Type>, LambdaExpression>();

        private readonly MapperConfiguration cfg;

        public MapBuilder(MapperConfiguration cfg)
        {
            this.cfg = cfg;
        }

        public Delegate GetMap(Type src, Type tgt)
        {
            var key = (src, tgt);
            if (maps.TryGetValue(key, out var map))
                return map;

            var ex = ResolveMap(src, tgt);
            if (ex == null)
                throw new MappingException(src, tgt, $"No map found.");

            return maps[key] = ex.Compile();
        }

        private LambdaExpression ResolveMap(Type src, Type tgt)
        {
            if (src == tgt)
                return null;

            var key = (src, tgt);
            if (raw.TryGetValue(key, out var map))
                return map;

            return raw[key] = BuildMap(src, tgt);
        }
    }
}