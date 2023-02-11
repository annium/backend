using System;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Processors;

internal static class NullableProcessor
{
    public static bool Process(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        if (!type.IsValueType)
        {
            if (nullability is Nullability.Unknown or Nullability.NotNullable)
                return false;

            ctx.Process(type, Nullability.NotNullable);

            return true;
        }

        var nullableBase = Nullable.GetUnderlyingType(type);
        if (nullableBase is null)
            return false;

        ctx.Process(nullableBase.ToContextualType(), Nullability.NotNullable);

        return true;
    }
}