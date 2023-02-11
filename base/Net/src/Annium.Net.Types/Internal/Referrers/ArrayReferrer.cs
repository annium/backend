using Annium.Net.Types.Internal.Helpers;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Referrers;

internal static class ArrayReferrer
{
    public static IRef? GetRef(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        if (!MapperConfig.IsArray(type))
            return null;

        var elementType = ArrayHelper.ResolveElementType(type);
        var valueRef = ctx.GetRef(elementType);

        return new ArrayRef(valueRef);
    }
}