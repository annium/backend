using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Annium.Core.Mapper.Internal.Builders
{
    internal partial class MapBuilder
    {
        private Func<Expression, Expression> BuildAssignmentMap(Type src, Type tgt, Map cfg) => source =>
        {
            // get source and target type properties
            var sources = src.GetProperties();
            var targets = tgt.GetProperties();
            // if any target properties are configured to be ignored - simply omit them from mapping
            if (cfg?.Ignores.Count() > 0)
                targets = targets
                    .Where(target => !cfg.Ignores.Any(ignored =>
                        ignored.DeclaringType == target.DeclaringType &&
                        ignored.PropertyType == target.PropertyType &&
                        ignored.Name == target.Name
                    ))
                    .ToArray();

            // defined instance and create initial assignemnt expression
            var instance = Expression.Variable(tgt);
            var init = Expression.Assign(instance, Expression.New(tgt.GetConstructor(Type.EmptyTypes)));

            // for each target property - resolve assignment expression
            var assignments = targets
                .Select<PropertyInfo, Expression>(target =>
                {
                    // if target field is explicitly configured in mapping - use that mapping
                    if (cfg?.Fields.ContainsKey(target) ?? false)
                        return Expression.Assign(Expression.Property(instance, target), _repacker.Repack(cfg!.Fields[target].Body)(source));

                    // otherwise - target field must match respective source field
                    var prop = sources.FirstOrDefault(p => p.Name.ToLowerInvariant() == target.Name.ToLowerInvariant()) ??
                        throw new MappingException(src, tgt, $"No property found for target property {target}");

                    // resolve map for conversion and use it, if necessary
                    var map = ResolveMap(prop.PropertyType, target.PropertyType);
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

        private Func<Expression, Expression> BuildAssignmentMap(Type src, Type tgt) => source =>
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
                    var map = ResolveMap(prop.PropertyType, target.PropertyType);
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