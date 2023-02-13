using Annium.Net.Types.Internal.Helpers;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Referrers;

internal class RecordReferrer : IReferrer
{
    public IRef? GetRef(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        if (!MapperConfig.IsRecord(type))
            return null;

        var (keyType, valueType) = RecordHelper.ResolveElementType(type);

        var keyRef = ctx.GetRef(keyType);
        var valueRef = ctx.GetRef(valueType);

        return new RecordRef(keyRef, valueRef);
    }
}