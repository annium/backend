using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Annium.Core.Mapper.Internal
{
    internal interface IMapConfiguration
    {
        LambdaExpression? MapWith { get; }
        IReadOnlyDictionary<PropertyInfo, LambdaExpression> MemberMaps { get; }
        IReadOnlyCollection<PropertyInfo> IgnoredMembers { get; }
    }
}