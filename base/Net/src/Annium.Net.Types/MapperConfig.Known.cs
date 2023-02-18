using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Net.Types.Internal.Extensions;
using Namotion.Reflection;

namespace Annium.Net.Types;

public static partial class MapperConfig
{
    private static readonly List<Predicate<Type>> Known = new();

    public static void RegisterKnown(Predicate<Type> matcher)
    {
        Known.Add(matcher);
    }

    public static bool IsKnown(Type type)
    {
        var pure = type.GetPure();

        return Known.Any(match => match(pure));
    }

    internal static bool IsKnown(ContextualType type) => IsIgnored(type.Type);
}