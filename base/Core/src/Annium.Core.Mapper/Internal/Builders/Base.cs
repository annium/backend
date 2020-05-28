using System;
using System.Linq.Expressions;

namespace Annium.Core.Mapper.Internal.Builders
{
    internal partial class MapBuilder
    {
        private Func<Expression, Expression> BuildMap(Type src, Type tgt, Map cfg) => source =>
        {
            if (GetEnumerableElementType(src) != null && GetEnumerableElementType(tgt) != null)
                return BuildEnumerableMap(src, tgt) (source);

            if (tgt.IsAbstract || tgt.IsInterface)
                return BuildResolutionMap(src, tgt) (source);

            if (tgt.GetConstructor(Type.EmptyTypes) == null)
                return BuildConstructorMap(src, tgt, cfg) (source);

            return BuildAssignmentMap(src, tgt, cfg) (source);
        };
    }
}