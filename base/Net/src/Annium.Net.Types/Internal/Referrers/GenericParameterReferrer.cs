using Annium.Net.Types.Models;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Referrers;

internal static class GenericParameterReferrer
{
    public static ModelRef? GetRef(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        return type.Type.IsGenericParameter ? new ModelRef(type.Type.Name) : null;
    }
}