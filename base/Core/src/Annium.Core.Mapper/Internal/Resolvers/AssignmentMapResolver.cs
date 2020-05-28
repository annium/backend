using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Annium.Core.Mapper.Internal.Resolvers
{
    internal class AssignmentMapResolver : IMapResolver
    {
        public bool CanResolveMap(Type src, Type tgt) => true;

        public Mapping ResolveMap(Type src, Type tgt, IMappingContext ctx) => source =>
        {
            // get source and target type properties
            var sources = src.GetProperties();
            var targets = tgt.GetProperties();

            // defined instance and create initial assignemnt expression
            var instance = Expression.Variable(tgt);
            var init = Expression.Assign(instance, Expression.New(tgt.GetConstructor(Type.EmptyTypes)));

            // for each target property - resolve assignment expression
            var assignments = targets
                .Select<PropertyInfo, Expression>(target =>
                {
                    // target field must match respective source field
                    var prop = sources.FirstOrDefault(p => p.Name.ToLowerInvariant() == target.Name.ToLowerInvariant()) ??
                        throw new MappingException(src, tgt, $"No property found for target property {target}");

                    // resolve map for conversion and use it, if necessary
                    var map = ctx.ResolveMapping(prop.PropertyType, target.PropertyType);
                    if (map is null)
                        return Expression.Assign(Expression.Property(instance, target), Expression.Property(source, prop));

                    return Expression.Assign(Expression.Property(instance, target), map(Expression.Property(source, prop)));
                })
                .ToArray();

            // if src is struct - things are simpler, no null-checking
            if (src.IsValueType)
                return Expression.Block(
                    new[] { instance },
                    new Expression[] { init }
                        .Concat(assignments)
                        .Concat(new Expression[] { instance })
                );

            // define labeled return expression, that will express early return null-checking statement
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
                new[] { instance },
                new Expression[] { nullCheck, init }
                    .Concat(assignments)
                    .Concat(new Expression[] { result, returnLabel })
            );
        };
    }
}