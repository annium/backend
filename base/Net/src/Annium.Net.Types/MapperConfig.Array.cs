using System;
using System.Collections.Generic;
using Annium.Core.Primitives;
using Annium.Core.Reflection;
using Annium.Net.Types.Internal.Extensions;
using Namotion.Reflection;

namespace Annium.Net.Types;

public static partial class MapperConfig
{
    internal static readonly Type BaseArrayType = typeof(IEnumerable<>);
    private static readonly HashSet<Type> ArrayTypes = new();

    private static void RegisterArrays()
    {
        RegisterArray(typeof(IEnumerable<>));
        RegisterArray(typeof(IReadOnlyCollection<>));
        RegisterArray(typeof(ICollection<>));
        RegisterArray(typeof(IReadOnlyList<>));
        RegisterArray(typeof(IList<>));
        RegisterArray(typeof(IReadOnlySet<>));
        RegisterArray(typeof(ISet<>));
        RegisterArray(typeof(List<>));
        RegisterArray(typeof(HashSet<>));
    }

    public static void RegisterArray(Type type)
    {
        if (type != type.TryGetPure())
            throw new ArgumentException($"Can't register type {type.FriendlyName()} as array type");

        if (type != BaseArrayType && !type.IsDerivedFrom(BaseArrayType))
            throw new ArgumentException($"Type {type.FriendlyName()} doesn't implement {BaseArrayType.FriendlyName()}");

        if (!ArrayTypes.Add(type))
            throw new ArgumentException($"Type {type.FriendlyName()} is already registered as array type");
    }

    internal static bool IsArray(ContextualType type)
    {
        return type.Type.IsArray || !ArrayTypes.Contains(type.Type.GetPure());
    }
}