using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Core.Reflection;
using Annium.Net.Types.Internal.Extensions;
using Namotion.Reflection;

namespace Annium.Net.Types;

public static partial class MapperConfig
{
    private static readonly Dictionary<Type, IgnoreConfig> Ignored = new();

    private static void RegisterIgnored()
    {
        // basic types
        RegisterIgnored(typeof(object), ignoreDerived: false);
        RegisterIgnored(typeof(ValueType), ignoreDerived: false);
        // enumerable interfaces
        RegisterIgnored(typeof(IEnumerable<>), ignoreDerived: false);
        RegisterIgnored(typeof(ICollection<>), ignoreDerived: false);
        RegisterIgnored(typeof(IReadOnlyCollection<>), ignoreDerived: false);
        // dictionary interfaces
        RegisterIgnored(typeof(IReadOnlyDictionary<,>), ignoreDerived: false);
        RegisterIgnored(typeof(IDictionary<,>), ignoreDerived: false);
        // base type interfaces
        RegisterIgnored(typeof(IComparable<>), ignoreDerived: false);
        RegisterIgnored(typeof(IEquatable<>), ignoreDerived: false);
        // low-level interfaces
        RegisterIgnored(typeof(ISpanParsable<>), ignoreDerived: false);
        RegisterIgnored(typeof(IParsable<>), ignoreDerived: false);
        // tasks
        RegisterIgnored(typeof(Task), ignoreDerived: true);
        RegisterIgnored(typeof(Task<>), ignoreDerived: true);
        RegisterIgnored(typeof(ValueTask), ignoreDerived: true);
        RegisterIgnored(typeof(ValueTask<>), ignoreDerived: true);
        // custom basic interfaces
        RegisterIgnored(typeof(ICopyable<>), ignoreDerived: false);
    }

    public static void RegisterIgnored(Type type, bool ignoreDerived)
    {
        if (type != type.TryGetPure())
            throw new ArgumentException($"Can't register type {type.FriendlyName()} as ignored type");

        if (!Ignored.TryAdd(type, new IgnoreConfig(ignoreDerived)))
            throw new ArgumentException($"Type {type.FriendlyName()} is already ignored");
    }

    public static bool IsIgnored(Type type)
    {
        var pure = type.GetPure();

        return Ignored.ContainsKey(pure) || IsDerivedIgnored(pure);
    }

    internal static bool IsIgnored(ContextualType type) => IsIgnored(type.Type);

    private static bool IsDerivedIgnored(Type type)
    {
        foreach (var (ignored, config) in Ignored)
        {
            if (ignored is { IsClass: false, IsInterface: false } || !config.IgnoreDerived)
                continue;

            if (type.IsDerivedFrom(ignored))
                return true;
        }

        return false;
    }

    private record IgnoreConfig(bool IgnoreDerived);
}