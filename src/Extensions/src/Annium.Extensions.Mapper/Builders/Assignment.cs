using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Annium.Extensions.Mapper
{
    public partial class MapBuilder
    {
        private LambdaExpression BuildAssignmentMap(Type src, Type tgt, ParameterExpression source)
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

                    return Expression.Assign(Expression.Property(instance, target), Expression.Invoke(map, Expression.Property(source, prop)));
                })
                .ToArray();

            var body = Expression.Block(
                new [] { instance },
                new Expression[] { init }
                .Concat(assignments)
                .Concat(new [] { instance })
            );

            return Expression.Lambda(body, source);
        }
    }
}