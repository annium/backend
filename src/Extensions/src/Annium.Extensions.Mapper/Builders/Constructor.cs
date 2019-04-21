using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Annium.Extensions.Primitives;

namespace Annium.Extensions.Mapper
{
    public partial class MapBuilder
    {
        private Expression BuildConstructorMap(Type src, Type tgt, Expression source)
        {
            var constructor = tgt.GetConstructors().OrderByDescending(c => c.GetParameters().Length).First();

            var properties = src.GetProperties();
            var parameters = constructor.GetParameters();
            var values = parameters
                .Select<ParameterInfo, Expression>(param =>
                {
                    var prop = properties.FirstOrDefault(p => p.Name.CamelCase() == param.Name) ??
                        throw new MappingException(src, tgt, $"No property found for constructor parameter {param}");

                    var map = ResolveMap(prop.PropertyType, param.ParameterType, Expression.Property(source, prop));
                    if (map == null)
                        return Expression.Property(source, prop);

                    return map;

                })
                .ToArray();

            return Expression.New(constructor, values);
        }
    }
}