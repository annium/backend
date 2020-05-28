using System;
using System.Linq.Expressions;

namespace Annium.Core.Mapper.Internal.Builders
{
    internal partial class MapBuilder
    {
        private Func<Expression, Expression> BuildMap(Type src, Type tgt, Map cfg) => source =>
        {
            if (GetEnumerableElementType(src) != null && GetEnumerableElementType(tgt) != null)
                return BuildEnumerableMap(src, tgt)(source);

            if (tgt.IsAbstract || tgt.IsInterface)
                return BuildResolutionMap(src, tgt)(source);

            if (tgt.GetConstructor(Type.EmptyTypes) is null)
                return BuildConstructorMap(src, tgt, cfg)(source);

            return BuildAssignmentMap(src, tgt, cfg)(source);
        };

        private Func<Expression, Expression> BuildMap(Type src, Type tgt) => source =>
        {
            if (GetEnumerableElementType(src) != null && GetEnumerableElementType(tgt) != null)
                return BuildEnumerableMap(src, tgt)(source);

            if (tgt.IsAbstract || tgt.IsInterface)
                return BuildResolutionMap(src, tgt)(source);

            if (tgt.GetConstructor(Type.EmptyTypes) is null)
                return BuildConstructorMap(src, tgt)(source);

            return BuildAssignmentMap(src, tgt)(source);
        };
    }
}