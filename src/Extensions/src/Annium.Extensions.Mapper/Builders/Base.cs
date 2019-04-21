using System;
using System.Linq.Expressions;

namespace Annium.Extensions.Mapper
{
    public partial class MapBuilder
    {
        private Expression BuildMap(Type src, Type tgt, Expression source)
        {
            if (tgt.IsAbstract || tgt.IsInterface)
                return BuildResolutionMap(src, tgt, source);

            if (GetEnumerableElementType(src) != null && GetEnumerableElementType(tgt) != null)
                return BuildEnumerableMap(src, tgt, source);

            if (tgt.GetConstructor(Type.EmptyTypes) == null)
                return BuildConstructorMap(src, tgt, source);

            return BuildAssignmentMap(src, tgt, source);
        }
    }
}