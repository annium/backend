using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Annium.Core.Mapper.Internal
{
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