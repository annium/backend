using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Annium.Core.Mapper
{
    public interface IMapConfiguration
    {
        Func<IMapContext, LambdaExpression>? ContextualMapWith { get; }
        LambdaExpression? MapWith { get; }
        IReadOnlyDictionary<PropertyInfo, LambdaExpression> MemberMaps { get; }
        IReadOnlyCollection<PropertyInfo> IgnoredMembers { get; }
    }
}