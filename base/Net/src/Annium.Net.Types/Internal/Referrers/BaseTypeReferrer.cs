using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Referrers;

internal class BaseTypeReferrer : IReferrer
{
    public IRef? GetRef(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        return ctx.Config.GetBaseTypeRefFor(type.Type);
    }
}