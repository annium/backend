using System;
using System.Linq.Expressions;
using Annium.Core.Application.Types;

namespace Annium.Core.Mapper
{
    internal partial class MapBuilder
    {
        private Func<Expression, Expression> BuildResolutionMap(Type src, Type tgt, Map cfg) => (Expression source) =>
        {
            var returnTarget = Expression.Label(tgt);
            var defaultValue = Expression.Default(tgt);
            var returnExpression = Expression.Return(returnTarget, defaultValue, tgt);
            var returnLabel = Expression.Label(returnTarget, defaultValue);

            var nullCheck = Expression.IfThen(
                Expression.Equal(source, Expression.Default(src)),
                returnExpression
            );

            var type = Expression.Variable(typeof(Type));
            var resolveFn = typeof(TypeManager).GetMethod(nameof(TypeManager.ResolveBySignature), new [] { typeof(object), typeof(Type), typeof(bool) });
            var resolution = Expression.Assign(type, Expression.Call(Expression.Constant(typeManager), resolveFn, source, Expression.Constant(tgt), Expression.Constant(true)));

            var map = Expression.Variable(typeof(Delegate));
            var mapFn = typeof(MapBuilder).GetMethod(nameof(MapBuilder.GetMap));
            var srcEx = Expression.Call(source, typeof(object).GetMethod(nameof(object.GetType)));
            var mapping = Expression.Assign(map, Expression.Call(Expression.Constant(this), mapFn, srcEx, type));

            var invokeFn = typeof(Delegate).GetMethod(nameof(Delegate.DynamicInvoke));
            var result = Expression.Call(map, invokeFn, Expression.NewArrayInit(typeof(object), source));
            var instance = Expression.Variable(tgt);
            var assignment = Expression.Assign(instance, Expression.Convert(result, tgt));

            var returnedResult = Expression.Return(returnTarget, instance, tgt);

            return Expression.Block(
                new [] { type, map, instance },
                nullCheck,
                resolution,
                mapping,
                assignment,
                returnedResult,
                returnLabel
            );
        };
    }
}