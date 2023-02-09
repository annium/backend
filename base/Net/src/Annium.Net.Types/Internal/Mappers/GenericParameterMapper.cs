using System;
using Annium.Core.Internal;
using Annium.Net.Types.Models;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Mappers;

internal static class GenericParameterMapper
{
    public static ITypeModel? Map(ContextualType type)
    {
        if (!type.Type.IsGenericParameter)
            return null;

        var model = new GenericParameterModel(type.TypeName);
        Log.Trace($"Mapped {type} -> {model}");

        return model;
    }
}