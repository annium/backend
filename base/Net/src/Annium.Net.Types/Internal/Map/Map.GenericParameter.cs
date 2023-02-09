using System;
using Annium.Net.Types.Models;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Map;

internal static partial class Map
{
    private static ITypeModel? ToGenericParameter(ContextualType type)
    {
        if (!type.Type.IsGenericParameter)
            return null;

        var model = new GenericParameterModel(type.TypeName);
        Console.WriteLine($"Mapped {type} -> {model}");

        return model;
    }
}