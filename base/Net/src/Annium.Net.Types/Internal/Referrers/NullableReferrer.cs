using System;
using Annium.Net.Types.Models;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Referrers;

internal static class NullableReferrer
{
    public static ModelRef? GetRef(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        if (!type.IsValueType)
            return nullability is Nullability.NotNullable ? null : ctx.GetRef(type, Nullability.NotNullable);

        var nullableBase = Nullable.GetUnderlyingType(type);
        return nullableBase is null ? null : ctx.GetRef(nullableBase.ToContextualType(), Nullability.NotNullable);
    }
}