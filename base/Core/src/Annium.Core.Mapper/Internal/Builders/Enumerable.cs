using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Annium.Core.Mapper.Internal
{
    internal partial class MapBuilder
    {
        private Func<Expression, Expression> BuildEnumerableMap(Type src, Type tgt, Map cfg) => (Expression source) =>
        {
            var srcEl = GetEnumerableElementType(src);
            var tgtEl = GetEnumerableElementType(tgt);

            if (tgt.IsInterface)
            {
                var def = tgt.GetGenericTypeDefinition();
                if (def == typeof(ICollection<>) || def == typeof(IReadOnlyCollection<>) || def == typeof(IEnumerable<>))
                    tgt = tgtEl.MakeArrayType();
                if (def == typeof(IList<>) || def == typeof(IReadOnlyList<>))
                    tgt = typeof(List<>).MakeGenericType(tgt.GenericTypeArguments);
                if (def == typeof(IDictionary<,>) || def == typeof(IReadOnlyDictionary<,>))
                    tgt = typeof(Dictionary<,>).MakeGenericType(tgt.GenericTypeArguments);
            }

            var select = typeof(Enumerable).GetMethods()
                .First(m => m.Name == nameof(Enumerable.Select))
                .MakeGenericMethod(srcEl, tgtEl);
            var toArray = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray))!.MakeGenericMethod(tgtEl);
            var param = Expression.Parameter(srcEl);
            var map = ResolveMap(srcEl, tgtEl);
            var selection = map is null ?
                source :
                Expression.Call(select, source, Expression.Lambda(map(param), param));
            var result = Expression.Condition(
                Expression.Equal(source, Expression.Default(src)),
                Expression.NewArrayInit(tgtEl),
                Expression.Call(toArray, selection)
            );

            if (tgt.IsArray)
                return result;

            var parameter = typeof(IEnumerable<>).MakeGenericType(tgtEl);
            var constructor = tgt.GetConstructor(new[] { parameter });
            if (constructor == null)
                throw new MappingException(src, tgt, $"No constructor with single {parameter} parameter found.");

            return Expression.New(constructor, selection);
        };

        private Type GetEnumerableElementType(Type type)
        {
            if (type.IsArray)
                return type.GetElementType()!;

            if (type.GenericTypeArguments.Length == 0)
                return null!;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return type.GenericTypeArguments[0];

            var iface = type.GetTypeInfo().ImplementedInterfaces
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            return iface?.GenericTypeArguments[0]!;
        }
    }
}