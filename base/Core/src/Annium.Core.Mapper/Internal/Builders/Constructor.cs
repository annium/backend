using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Annium.Core.Mapper.Internal.Builders
{
    internal partial class MapBuilder
    {
        private Func<Expression, Expression> BuildConstructorMap(Type src, Type tgt, Map cfg) => source =>
        {
            // find constructor with biggest number of parameters (pretty simple logic for now)
            var constructor = tgt
                .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .OrderByDescending(c => c.GetParameters().Length)
                .First();

            // get source properties and constructor parameters
            var sources = src.GetProperties();
            var parameters = constructor.GetParameters();

            // map parameters to their value evaluation expressions
            var values = parameters
                .Select(param =>
                {
                    var paramName = param.Name!.ToLowerInvariant();

                    // if respective property is ignored - use default value for parameter
                    if (cfg?.Ignores.Any(i => i.Name.ToLowerInvariant() == paramName) ?? false)
                        return Expression.Default(param.ParameterType);

                    // if target field is explicitly configured in mapping - use that mapping
                    if (cfg?.Fields.Any(p => p.Key.Name.ToLowerInvariant() == paramName) ?? false)
                        return _repacker.Repack(cfg!.Fields.First(p => p.Key.Name.ToLowerInvariant() == paramName).Value.Body)(source);

                    // otherwise - parameter must match respective source field
                    var prop = sources.FirstOrDefault(p => p.Name.ToLowerInvariant() == paramName) ??
                        throw new MappingException(src, tgt, $"No property found for constructor parameter {param}");

                    // resolve map for conversion and use it, if necessary
                    var map = ResolveMap(prop.PropertyType, param.ParameterType);
                    if (map is null)
                        return Expression.Property(source, prop);

                    return map(Expression.Property(source, prop));
                })
                .ToArray();

            if (src.IsValueType)
                return Expression.New(constructor, values);

            return Expression.Condition(
                Expression.Equal(source, Expression.Default(src)),
                Expression.Default(tgt),
                Expression.New(constructor, values)
            );
        };
    }
}