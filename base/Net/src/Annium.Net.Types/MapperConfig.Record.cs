using System;
using System.Collections.Generic;
using Annium.Core.Primitives;
using Annium.Core.Reflection;
using Namotion.Reflection;

namespace Annium.Net.Types;

public static partial class MapperConfig
{
    internal static readonly Type BaseRecordType = typeof(IEnumerable<>).MakeGenericType(typeof(KeyValuePair<,>));
    private static readonly HashSet<Type> RecordTypes = new();

    private static void RegisterRecords()
    {
        RegisterRecord(typeof(IDictionary<,>));
        RegisterRecord(typeof(IReadOnlyDictionary<,>));
        RegisterRecord(typeof(Dictionary<,>));
    }

    public static void RegisterRecord(Type type)
    {
        if (type.IsGenericParameter)
            throw new ArgumentException($"Can't register generic parameter {type.FriendlyName()} as Record type");

        if (type is { IsGenericType: true, IsGenericTypeDefinition: false })
            throw new ArgumentException($"Can't register generic type {type.FriendlyName()} as Record type");

        if (type != BaseArrayType && !type.IsDerivedFrom(BaseArrayType))
            throw new ArgumentException($"Type {type.FriendlyName()} doesn't implement {BaseRecordType.FriendlyName()}");

        if (!RecordTypes.Add(type))
            throw new ArgumentException($"Type {type.FriendlyName()} is already registered as Record type");
    }

    internal static bool IsRecord(ContextualType type)
    {
        return RecordTypes.Contains(type.Type.IsGenericType ? type.Type.GetGenericTypeDefinition() : type.Type);
    }
}