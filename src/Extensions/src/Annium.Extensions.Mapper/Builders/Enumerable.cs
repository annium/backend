using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Annium.Extensions.Mapper
{
    public partial class MapBuilder
    {
        private Func<Expression, Expression> BuildEnumerableMap(Type src, Type tgt) => (Expression source) =>
        {
            var srcEl = GetEnumerableElementType(src);
            var tgtEl = GetEnumerableElementType(tgt);

            var select = typeof(Enumerable).GetMethods()
                .First(m => m.Name == nameof(Enumerable.Select))
                .MakeGenericMethod(srcEl, tgtEl);
            var param = Expression.Parameter(srcEl);
            var map = ResolveMap(srcEl, tgtEl) (param);
            var lambda = Expression.Lambda(map, param);
            var selection = Expression.Call(select, source, lambda);

            return tgt.IsArray? BuildArrayMap() : BuildConstructorMap();

            Expression BuildArrayMap()
            {
                var toArray = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray)).MakeGenericMethod(tgtEl);

                return Expression.Call(toArray, selection);
            }

            Expression BuildConstructorMap()
            {
                var parameter = typeof(IEnumerable<>).MakeGenericType(tgtEl);
                var constructor = tgt.GetConstructor(new [] { parameter });
                if (constructor == null)
                    throw new MappingException(src, tgt, $"No constructor with single {parameter} parameter found.");

                return Expression.New(constructor, selection);
            }
        };

        private Type GetEnumerableElementType(Type type)
        {
            if (type.IsArray)
                return type.GetElementType();

            if (type.GenericTypeArguments.Length == 0)
                return null;

            return type.GetTypeInfo().ImplementedInterfaces
                .FirstOrDefault(i => i.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ?
                .GenericTypeArguments[0];
        }
    }
}