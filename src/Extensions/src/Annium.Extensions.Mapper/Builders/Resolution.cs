using System;
using System.Linq.Expressions;

namespace Annium.Extensions.Mapper
{
    public partial class MapBuilder
    {
        private Expression BuildResolutionMap(Type src, Type tgt, Expression source)
        {
            var type = Expression.Variable(typeof(Type));
            var resolveFn = typeof(TypeResolver).GetMethod(nameof(TypeResolver.Resolve));
            var resolution = Expression.Assign(type, Expression.Call(Expression.Constant(typeResolver), resolveFn, source, Expression.Constant(tgt)));

            var map = Expression.Variable(typeof(Delegate));
            var mapFn = typeof(MapBuilder).GetMethod(nameof(MapBuilder.GetMap));
            var srcEx = Expression.Call(source, typeof(object).GetMethod(nameof(object.GetType)));
            var mapping = Expression.Assign(map, Expression.Call(Expression.Constant(this), mapFn, srcEx, type));

            var invokeFn = typeof(Delegate).GetMethod(nameof(Delegate.DynamicInvoke));
            var result = Expression.Call(map, invokeFn, Expression.NewArrayInit(typeof(object), source));
            var instance = Expression.Variable(tgt);
            var assignment = Expression.Assign(instance, Expression.Convert(result, tgt));

            return Expression.Block(
                new [] { type, map, instance },
                resolution,
                mapping,
                assignment,
                instance
            );
        }
    }
}