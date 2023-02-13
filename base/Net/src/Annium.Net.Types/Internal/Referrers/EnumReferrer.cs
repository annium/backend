using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Referrers;

internal class EnumReferrer : IReferrer
{
    public IRef? GetRef(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        return type.Type.IsEnum ? ctx.RequireRef(type) : null;
    }
}