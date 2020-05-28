using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Annium.Core.Reflection;

namespace Annium.Core.Mapper
{
    public class Map<S, T> : Map
    {
        public new void Type(Expression<Func<S, T>> map)
        {
            base.Type = map;
        }

        public Map<S, T> Field(
            Expression<Func<S, object>> map,
            Expression<Func<T, object>> field
        )
        {
            fields[TypeHelper.ResolveProperty(field)] = map;

            return this;
        }

        public Map<S, T> Ignore(
            Expression<Func<T, object>> field
        )
        {
            ignores.Add(TypeHelper.ResolveProperty(field));

            return this;
        }
    }

    public class Map
    {
        internal LambdaExpression? Type { get; set; }

        internal IReadOnlyDictionary<PropertyInfo, LambdaExpression> Fields => fields;

        internal IEnumerable<PropertyInfo> Ignores => ignores;

        protected readonly Dictionary<PropertyInfo, LambdaExpression> fields = new Dictionary<PropertyInfo, LambdaExpression>();

        protected readonly HashSet<PropertyInfo> ignores = new HashSet<PropertyInfo>();
    }
}