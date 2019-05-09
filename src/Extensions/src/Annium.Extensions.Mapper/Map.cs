using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Annium.Extensions.Mapper
{
    public class MapConfiguration<S, T> : Map
    {
        public new MapConfiguration<S, T> Type(
            Expression<Func<S, T>> map
        )
        {
            base.Type = map;

            return this;
        }

        public MapConfiguration<S, T> Field(
            Expression<Func<S, object>> map,
            Expression<Func<T, object>> field
        )
        {
            fields[resolve(field)] = map;

            return this;
        }

        public MapConfiguration<S, T> Ignore(
            Expression<Func<T, object>> field
        )
        {
            ignores.Add(resolve(field));

            return this;
        }

        private PropertyInfo resolve(Expression<Func<T, object>> map)
        {
            if (map.Body is MemberExpression member)
                return resolve(member);

            if (map.Body is UnaryExpression unary)
                return resolve(unary);

            throw new ArgumentException($"Can't resolve property from {map}");
        }

        private PropertyInfo resolve(MemberExpression ex)
        {
            if (ex.Member is PropertyInfo property)
                return property;

            throw new ArgumentException($"{ex} is not a property access exception");
        }

        private PropertyInfo resolve(UnaryExpression ex)
        {
            if (ex.Operand is MemberExpression member)
                return resolve(member);

            throw new ArgumentException($"{ex} is not a property access exception");
        }
    }

    public class Map
    {
        internal LambdaExpression Type { get; set; }

        internal IReadOnlyDictionary<PropertyInfo, LambdaExpression> Fields => fields;

        internal IEnumerable<PropertyInfo> Ignores => ignores;

        protected Dictionary<PropertyInfo, LambdaExpression> fields = new Dictionary<PropertyInfo, LambdaExpression>();

        protected HashSet<PropertyInfo> ignores = new HashSet<PropertyInfo>();
    }
}