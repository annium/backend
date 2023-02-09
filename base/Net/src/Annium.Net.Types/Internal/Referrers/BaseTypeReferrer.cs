using Annium.Net.Types.Models;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Referrers;

internal static class BaseTypeReferrer
{
    public static ModelRef? GetRef(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        return BaseType.GetFor(type.Type);
    }
}