using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Annium.Core.Mapper
{
    public interface IMapConfiguration
    {
        LambdaExpression? MapWith { get; }
        IReadOnlyDictionary<PropertyInfo, LambdaExpression> MemberMaps { get; }
        IReadOnlyCollection<PropertyInfo> IgnoredMembers { get; }
    }
}