using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Annium.Core.Reflection;

namespace Annium.Core.Mapper.Internal
{
    internal class MapConfiguration<S, T> : IMapConfiguration<S, T>
    {
        public IMapConfiguration View => _view;
        private readonly MapConfiguration _view = new MapConfiguration();


        public void With(Expression<Func<S, T>> map)
        {
            _view.SetMapWith(map);
        }

        public IMapConfiguration<S, T> For(Expression<Func<T, object>> members, Expression<Func<S, object>> map)
        {
            var properties = TypeHelper.ResolveProperties(members);

            _view.AddMapWithFor(properties, map);

            return this;
        }

        public IMapConfiguration<S, T> Ignore(Expression<Func<T, object>> members)
        {
            var properties = TypeHelper.ResolveProperties(members);

            _view.Ignore(properties);

            return this;
        }
    }

    internal class MapConfiguration : IMapConfiguration
    {
        public static IMapConfiguration Empty { get; } = new MapConfiguration();

        public LambdaExpression? MapWith { get; private set; }
        public IReadOnlyDictionary<PropertyInfo, LambdaExpression> MemberMaps => _memberMaps;
        public IReadOnlyCollection<PropertyInfo> IgnoredMembers => _ignoredMembers;
        private readonly Dictionary<PropertyInfo, LambdaExpression> _memberMaps = new Dictionary<PropertyInfo, LambdaExpression>();
        private readonly HashSet<PropertyInfo> _ignoredMembers = new HashSet<PropertyInfo>();

        public void SetMapWith(LambdaExpression mapWith)
        {
            MapWith = mapWith;
        }

        public void AddMapWithFor(IReadOnlyCollection<PropertyInfo> properties, LambdaExpression mapWith)
        {
            foreach (var property in properties)
                _memberMaps[property] = mapWith;
        }

        public void Ignore(IReadOnlyCollection<PropertyInfo> properties)
        {
            foreach (var property in properties)
                _ignoredMembers.Add(property);
        }
    }
}