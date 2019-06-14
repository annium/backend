using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Annium.Core.Application.Types
{
    public static class TypeHelper
    {
        public static PropertyInfo ResolveProperty<TObject>(Expression<Func<TObject, object>> map) =>
            ResolveProperty<TObject, object>(map);

        public static PropertyInfo ResolveProperty<TObject, TField>(Expression<Func<TObject, TField>> map)
        {
            if (map.Body is MemberExpression member)
                return ResolveProperty(member);

            if (map.Body is UnaryExpression unary)
                return ResolveProperty(unary);

            throw new ArgumentException($"Can't resolve property from {map}");
        }

        private static PropertyInfo ResolveProperty(MemberExpression ex)
        {
            if (ex.Member is PropertyInfo property)
                return property;

            throw new ArgumentException($"{ex} is not a property access expression");
        }

        private static PropertyInfo ResolveProperty(UnaryExpression ex)
        {
            if (ex.Operand is MemberExpression member)
                return ResolveProperty(member);

            throw new ArgumentException($"{ex} is not a property access expression");
        }
    }
}