using System;
using System.Collections.Generic;
using Annium.Core.Internal;
using Annium.Core.Primitives;
using Annium.Net.Types.Extensions;
using Annium.Net.Types.Models;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Mappers;

internal static class EnumMapper
{
    public static ITypeModel? Map(ContextualType type)
    {
        if (!type.Type.IsEnum)
            return null;

        var names = Enum.GetNames(type);
        var rawValues = Enum.GetValuesAsUnderlyingType(type);

        var values = new Dictionary<string, long>();
        var i = 0;
        foreach (var value in rawValues)
            values[names[i++]] = Convert.ToInt64(value);

        var model = new EnumModel(type.GetNamespace(), type.Type.FriendlyName(), values);
        Log.Trace($"Mapped {type} -> {model}");

        return model;
    }
}