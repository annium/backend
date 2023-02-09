using System;
using Annium.Net.Types.Internal.Extensions;
using Annium.Net.Types.Models;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Map;

internal static partial class Map
{
    static Map()
    {
        RegisterIgnored();
        RegisterArrays();
        RegisterRecords();
    }

    public static ITypeModel ToModel(ContextualType type) => ToModel(type, type.IsNullable());

    private static ITypeModel ToModel(ContextualType type, bool isNullable)
    {
        Console.WriteLine($"Map {type}");
        return ToNullable(type, isNullable)
            ?? ToGenericParameter(type)
            ?? ToBaseType(type)
            ?? ToEnum(type)
            ?? ToSpecial(type)
            ?? ToArray(type)
            ?? ToRecord(type)
            ?? ToStruct(type);
    }
}