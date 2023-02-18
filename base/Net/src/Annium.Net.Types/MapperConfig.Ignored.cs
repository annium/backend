using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Net.Types.Internal.Extensions;
using Namotion.Reflection;

namespace Annium.Net.Types;

public static partial class MapperConfig
{
    private static readonly List<Predicate<Type>> Ignored = new();

    private static void RegisterIgnored()
    {
        // basic types
        RegisterIgnored(Match.Is(typeof(object)));
        RegisterIgnored(Match.Is(typeof(ValueType)));
        // enumerable interfaces
        RegisterIgnored(Match.Is(typeof(IEnumerable<>)));
        RegisterIgnored(Match.Is(typeof(ICollection<>)));
        RegisterIgnored(Match.Is(typeof(IReadOnlyCollection<>)));
        // dictionary interfaces
        RegisterIgnored(Match.Is(typeof(IReadOnlyDictionary<,>)));
        RegisterIgnored(Match.Is(typeof(IDictionary<,>)));
        // base type interfaces
        RegisterIgnored(Match.Is(typeof(IComparable<>)));
        RegisterIgnored(Match.Is(typeof(IEquatable<>)));
        // low-level interfaces
        RegisterIgnored(Match.Is(typeof(ISpanParsable<>)));
        RegisterIgnored(Match.Is(typeof(IParsable<>)));
        // tasks
        RegisterIgnored(Match.IsDerivedFrom(typeof(Task)));
        RegisterIgnored(Match.IsDerivedFrom(typeof(Task<>)));
        RegisterIgnored(Match.Is(typeof(ValueTask)));
        RegisterIgnored(Match.Is(typeof(ValueTask<>)));
        // custom basic interfaces
        RegisterIgnored(Match.Is(typeof(ICopyable<>)));
    }

    public static void RegisterIgnored(Predicate<Type> matcher)
    {
        Ignored.Add(matcher);
    }

    public static bool IsIgnored(Type type)
    {
        var pure = type.GetPure();

        return Ignored.Any(match => match(pure));
    }

    internal static bool IsIgnored(ContextualType type) => IsIgnored(type.Type);
}