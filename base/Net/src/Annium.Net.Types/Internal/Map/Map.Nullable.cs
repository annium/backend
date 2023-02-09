using System;
using Annium.Net.Types.Models;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Map;

internal static partial class Map
{
    private static ITypeModel? ToNullable(ContextualType type, bool isNullable)
    {
        ITypeModel model;
        if (type.IsValueType)
        {
            var nullableBase = Nullable.GetUnderlyingType(type);
            if (nullableBase is null)
                return null;

            model = new NullableModel(ToModel(nullableBase.ToContextualType(), false));
            Console.WriteLine($"Mapped {type} -> {model}");

            return model;
        }

        if (!isNullable)
            return null;

        model = new NullableModel(ToModel(type, false));
        Console.WriteLine($"Mapped {type} -> {model}");

        return model;
    }
}