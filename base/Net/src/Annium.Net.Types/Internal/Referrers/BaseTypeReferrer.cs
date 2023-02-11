using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Referrers;

internal static class BaseTypeReferrer
{
    public static IRef? GetRef(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        return BaseType.GetRefFor(type.Type);
    }
}