using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Annium.Extensions.Mapper
{
    internal partial class MapBuilder
    {
        private Func<Expression, Expression> BuildAssignmentMap(Type src, Type tgt, Map cfg) => (Expression source) =>
        {
            var sources = src.GetProperties();
            var targets = tgt.GetProperties();

            var instance = Expression.Variable(tgt);
            var init = Expression.Assign(instance, Expression.New(tgt.GetConstructor(Type.EmptyTypes)));
            var assignments = targets
                .Select<PropertyInfo, Expression>(target =>
                {
                    var prop = sources.FirstOrDefault(p => p.Name == target.Name) ??
                        throw new MappingException(src, tgt, $"No property found for target property {target}");

                    var map = ResolveMap(prop.PropertyType, target.PropertyType);
                    if (map == null)
                        return Expression.Assign(Expression.Property(instance, target), Expression.Property(source, prop));

                    return Expression.Assign(Expression.Property(instance, target), map(Expression.Property(source, prop)));
                })
                .ToArray();

            if (src.IsValueType)
                return Expression.Block(
                    new [] { instance },
                    new Expression[] { init }
                    .Concat(assignments)
                    .Concat(new Expression[] { instance })
                );

            var returnTarget = Expression.Label(tgt);
            var defaultValue = Expression.Default(tgt);
            var returnExpression = Expression.Return(returnTarget, defaultValue, tgt);
            var returnLabel = Expression.Label(returnTarget, defaultValue);

            var nullCheck = Expression.IfThen(
                Expression.Equal(source, Expression.Default(src)),
                returnExpression
            );

            var result = Expression.Return(returnTarget, instance, tgt);

            return Expression.Block(
                new [] { instance },
                new Expression[] { nullCheck, init }
                .Concat(assignments)
                .Concat(new Expression[] { result, returnLabel })
            );
        };
    }
}