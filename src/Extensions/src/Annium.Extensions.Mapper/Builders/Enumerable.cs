using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Annium.Extensions.Mapper
{
    public partial class MapBuilder
    {
        private LambdaExpression BuildEnumerableMap(Type src, Type tgt, ParameterExpression source)
        {
            var srcEl = GetEnumerableElementType(src);
            var tgtEl = GetEnumerableElementType(tgt);
            var map = ResolveMap(srcEl, tgtEl);

            var select = typeof(Enumerable).GetMethods()
                .First(m => m.Name == nameof(Enumerable.Select))
                .MakeGenericMethod(srcEl, tgtEl);
            var selection = Expression.Call(select, source, map);

            var body = tgt.IsArray? BuildArrayMap() : BuildConstructorMap();

            return Expression.Lambda(body, source);

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
        }

        private Type GetEnumerableElementType(Type type)
        {
            if (type.IsArray)
                return type.GetElementType();

            if (type.GenericTypeArguments.Length == 0)
                return null;

            return type.GetTypeInfo().ImplementedInterfaces
                .FirstOrDefault(i => i.GetGenericTypeDefinition() == typeof(IEnumerable<>))?
                .GenericTypeArguments[0];
        }
    }
}