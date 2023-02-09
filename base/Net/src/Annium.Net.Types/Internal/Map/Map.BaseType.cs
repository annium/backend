using System;
using Annium.Net.Types.Models;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Map;

internal static partial class Map
{
    private static ITypeModel? ToBaseType(ContextualType type)
    {
        var model = BaseType.GetFor(type.Type);
        if (model is null)
            return null;

        Console.WriteLine($"Mapped {type} -> {model}");

        return model;
    }
}