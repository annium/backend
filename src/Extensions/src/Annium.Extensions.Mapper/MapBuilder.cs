using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Annium.Extensions.Mapper
{
    internal partial class MapBuilder
    {
        private readonly IDictionary<ValueTuple<Type, Type>, Delegate> maps =
            new Dictionary<ValueTuple<Type, Type>, Delegate>();

        private readonly IDictionary<ValueTuple<Type, Type>, Func<Expression, Expression>> raw =
            new Dictionary<ValueTuple<Type, Type>, Func<Expression, Expression>>();

        private readonly TypeResolver typeResolver;

        public MapBuilder(
            MapperConfiguration cfg,
            TypeResolver typeResolver,
            Repacker repacker
        )
        {
            this.typeResolver = typeResolver;
            foreach (var(key, map) in cfg.Maps)
                raw[key] = repacker.Repack(map.Body);
        }

        public Delegate GetMap(Type src, Type tgt)
        {
            var key = (src, tgt);
            if (maps.TryGetValue(key, out var map))
                return map;

            var param = Expression.Parameter(src);
            var body = ResolveMap(src, tgt) (param);
            if (body == null)
                throw new MappingException(src, tgt, $"No map found.");

            var result = Expression.Lambda(body, param);

            return maps[key] = result.Compile();
        }

        private Func<Expression, Expression> ResolveMap(Type src, Type tgt)
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