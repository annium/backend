using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Annium.Extensions.Mapper
{
    public partial class MapBuilder
    {
        private readonly IDictionary<ValueTuple<Type, Type>, Delegate> maps =
            new Dictionary<ValueTuple<Type, Type>, Delegate>();

        private readonly IDictionary<ValueTuple<Type, Type>, Expression> raw =
            new Dictionary<ValueTuple<Type, Type>, Expression>();

        private readonly MapperConfiguration cfg;

        private readonly TypeResolver typeResolver;

        public MapBuilder(
            MapperConfiguration cfg,
            TypeResolver typeResolver
        )
        {
            this.cfg = cfg;
            this.typeResolver = typeResolver;
        }

        public Delegate GetMap(Type src, Type tgt)
        {
            var key = (src, tgt);
            if (maps.TryGetValue(key, out var map))
                return map;

            var param = Expression.Parameter(src);
            var body = ResolveMap(src, tgt, param);
            if (body == null)
                throw new MappingException(src, tgt, $"No map found.");

            var result = Expression.Lambda(body, param);

            return maps[key] = result.Compile();
        }

        private Expression ResolveMap(Type src, Type tgt, Expression param)
        {
            if (src == tgt)
                return null;

            var key = (src, tgt);
            if (raw.TryGetValue(key, out var map))
                return map;

            return raw[key] = BuildMap(src, tgt, param);
        }
    }
}