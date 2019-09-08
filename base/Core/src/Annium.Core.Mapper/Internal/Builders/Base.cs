using System;
using System.Linq.Expressions;

namespace Annium.Core.Mapper.Internal
{
    internal partial class MapBuilder
    {
        private Func<Expression, Expression> BuildMap(Type src, Type tgt, Map cfg) => (Expression source) =>
        {
            if (GetEnumerableElementType(src) != null && GetEnumerableElementType(tgt) != null)
                return BuildEnumerableMap(src, tgt, cfg) (source);

            if (tgt.IsAbstract || tgt.IsInterface)
                return BuildResolutionMap(src, tgt, cfg) (source);

            if (tgt.GetConstructor(Type.EmptyTypes) == null)
                return BuildConstructorMap(src, tgt, cfg) (source);

            return BuildAssignmentMap(src, tgt, cfg) (source);
        };
    }
}