using Annium.Net.Types.Internal.Helpers;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Processors;

internal static class ArrayProcessor
{
    public static bool Process(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        if (!MapperConfig.IsArray(type))
            return false;

        var elementType = ArrayHelper.ResolveElementType(type);
        ctx.Process(elementType);

        return true;
    }
}