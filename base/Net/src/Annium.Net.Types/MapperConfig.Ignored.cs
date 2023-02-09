using System;
using System.Collections.Generic;
using Annium.Core.Primitives;
using Annium.Net.Types.Internal.Extensions;
using Namotion.Reflection;

namespace Annium.Net.Types;

public static partial class MapperConfig
{
    private static readonly HashSet<Type> Ignored = new();

    private static void RegisterIgnored()
    {
        // basic types
        RegisterIgnored(typeof(object));
        RegisterIgnored(typeof(ValueType));
        // enumerable interfaces
        RegisterIgnored(typeof(IEnumerable<>));
        RegisterIgnored(typeof(ICollection<>));
        RegisterIgnored(typeof(IReadOnlyCollection<>));
        // dictionary interfaces
        RegisterIgnored(typeof(IReadOnlyDictionary<,>));
        RegisterIgnored(typeof(IDictionary<,>));
        // base type interfaces
        RegisterIgnored(typeof(IComparable<>));
        RegisterIgnored(typeof(IEquatable<>));
        // low-level interfaces
        RegisterIgnored(typeof(ISpanParsable<>));
        RegisterIgnored(typeof(IParsable<>));
    }

    public static void RegisterIgnored(Type type)
    {
        if (type != type.TryGetPure())
            throw new ArgumentException($"Can't register type {type.FriendlyName()} as ignored type");

        if (!Ignored.Add(type))
            throw new ArgumentException($"Type {type.FriendlyName()} is already ignored");
    }

    internal static bool IsIgnored(ContextualType type) => Ignored.Contains(type.Type.GetPure());
}