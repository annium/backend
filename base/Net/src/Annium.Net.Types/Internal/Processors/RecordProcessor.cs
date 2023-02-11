using Annium.Net.Types.Internal.Helpers;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Processors;

internal static class RecordProcessor
{
    public static bool Process(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        if (!MapperConfig.IsRecord(type))
            return false;

        var (keyType, valueType) = RecordHelper.ResolveElementType(type);
        ctx.Process(keyType);
        ctx.Process(valueType);

        return true;
    }
}