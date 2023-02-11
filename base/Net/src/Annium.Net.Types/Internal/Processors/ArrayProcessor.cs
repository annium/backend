using System;
using Annium.Core.Reflection;
using Annium.Net.Types.Internal.Extensions;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Processors;

internal static class ArrayProcessor
{
    public static bool Process(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        if (!MapperConfig.IsArray(type))
            return false;

        var elementType = type.Type.IsArray
            ? type.Type.GetElementType()?.ToContextualType()
            : type.Type.GetTargetImplementation(MapperConfig.BaseArrayType)?.ToContextualType().GenericArguments[0];

        ctx.Process(elementType ?? throw new InvalidOperationException($"Failed to resolve element type of {type.FriendlyName()}"));

        return true;
    }
}