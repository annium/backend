using System;
using Annium.Core.Internal;
using Annium.Core.Primitives;
using Annium.Core.Reflection;
using Annium.Net.Types.Models;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Mappers;

internal static class ArrayMapper
{
    public static ITypeModel? Map(ContextualType type, IMapperContext ctx)
    {
        if (!MapperConfig.IsArray(type))
            return null;

        var elementType = type.Type.IsArray
            ? type.Type.ToContextualType().GenericArguments[0] ?? throw new InvalidOperationException($"Failed to resolve element type of {type.Type.FriendlyName()}")
            : type.Type.GetTargetImplementation(MapperConfig.BaseArrayType)!.ToContextualType().GenericArguments[0];
        var elementTypeModel = ctx.Map(elementType);
        var model = new ArrayModel(elementTypeModel);
        Log.Trace($"Mapped {type} -> {model}");

        return model;
    }
}