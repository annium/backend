using System;
using System.Linq.Expressions;
using Annium.Core.Reflection;

namespace Annium.Core.Mapper.Internal
{
    internal class MapConfigurationBuilder<S, T> : IMapConfigurationBuilder<S, T>
    {
        public IMapConfiguration Result => _result;
        private readonly MapConfiguration _result = new MapConfiguration();


        public void With(Expression<Func<S, T>> map)
        {
            _result.SetMapWith(map);
        }

        public IMapConfigurationBuilder<S, T> For(Expression<Func<T, object>> members, Expression<Func<S, object>> map)
        {
            var properties = TypeHelper.ResolveProperties(members);

            _result.AddMapWithFor(properties, map);

            return this;
        }

        public IMapConfigurationBuilder<S, T> Ignore(Expression<Func<T, object>> members)
        {
            var properties = TypeHelper.ResolveProperties(members);

            _result.Ignore(properties);

            return this;
        }
    }
}