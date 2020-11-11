using System;
using System.Linq.Expressions;
using Annium.Core.Reflection;

namespace Annium.Core.Mapper.Internal
{
    internal class MapConfigurationBuilder<TS, T> : IMapConfigurationBuilder<TS, T>
    {
        public IMapConfiguration Result => _result;
        private readonly MapConfiguration _result = new MapConfiguration();


        public void With(Expression<Func<TS, T>> map)
        {
            _result.SetMapWith(map);
        }

        public IMapConfigurationBuilder<TS, T> For<TF>(Expression<Func<T, object>> members, Expression<Func<TS, TF>> map)
        {
            var properties = TypeHelper.ResolveProperties(members);

            _result.AddMapWithFor(properties, map);

            return this;
        }

        public IMapConfigurationBuilder<TS, T> Ignore(Expression<Func<T, object>> members)
        {
            var properties = TypeHelper.ResolveProperties(members);

            _result.Ignore(properties);

            return this;
        }
    }
}