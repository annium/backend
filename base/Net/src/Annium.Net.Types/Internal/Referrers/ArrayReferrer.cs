using System;
using Annium.Core.Reflection;
using Annium.Net.Types.Internal.Extensions;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Referrers;

internal static class ArrayReferrer
{
    public static IRef? GetRef(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        if (!MapperConfig.IsArray(type))
            return null;

        var elementType = type.Type.IsArray
            ? type.Type.GetElementType()?.ToContextualType()
            : type.Type.GetTargetImplementation(MapperConfig.BaseArrayType)?.ToContextualType().GenericArguments[0];

        var valueRef = ctx.GetRef(elementType ?? throw new InvalidOperationException($"Failed to resolve element type of {type.FriendlyName()}"));

        return new ArrayRef(valueRef);
    }
}