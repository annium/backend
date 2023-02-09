using Annium.Net.Types.Models;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Referrers;

internal static class EnumReferrer
{
    public static ModelRef? GetRef(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        return type.Type.IsEnum ? ctx.RequireRef(type) : null;
    }
}