using System;
using Annium.Core.Internal;
using Annium.Net.Types.Models;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Mappers;

internal static class NullableMapper
{
    public static ITypeModel? Map(ContextualType type, Nullability nullability, IMapperContext ctx)
    {
        ITypeModel model;
        if (type.IsValueType)
        {
            var nullableBase = Nullable.GetUnderlyingType(type);
            if (nullableBase is null)
                return null;

            model = new NullableModel(ctx.Map(nullableBase.ToContextualType(), Nullability.NotNullable));
            Log.Trace($"Mapped {type} -> {model}");

            return model;
        }

        if (nullability is Nullability.NotNullable)
            return null;

        model = new NullableModel(ctx.Map(type, nullability));
        Log.Trace($"Mapped {type} -> {model}");

        return model;
    }
}