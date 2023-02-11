using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.Primitives;
using Annium.Net.Types.Internal.Extensions;
using Namotion.Reflection;

namespace Annium.Net.Types;

public static partial class MapperConfig
{
    internal static readonly Type BaseRecordValueType = typeof(KeyValuePair<,>);
    private static readonly Type BaseRecordType = typeof(IEnumerable<>).MakeGenericType(BaseRecordValueType);
    private static readonly HashSet<Type> RecordTypes = new();

    private static void RegisterRecords()
    {
        RegisterRecord(typeof(IDictionary<,>));
        RegisterRecord(typeof(IReadOnlyDictionary<,>));
        RegisterRecord(typeof(Dictionary<,>));
    }

    public static void RegisterRecord(Type type)
    {
        if (type != type.TryGetPure())
            throw new ArgumentException($"Can't register type {type.FriendlyName()} as Record type");

        var arrayImplementation = type.GetInterfaces().SingleOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == BaseArrayType);
        if (arrayImplementation is null || !arrayImplementation.IsGenericType || arrayImplementation.GetGenericArguments()[0].GetGenericTypeDefinition() != BaseRecordValueType)
            throw new ArgumentException($"Type {type.FriendlyName()} doesn't implement {BaseRecordType.FriendlyName()}");

        if (!RecordTypes.Add(type))
            throw new ArgumentException($"Type {type.FriendlyName()} is already registered as Record type");
    }

    internal static bool IsRecord(ContextualType type)
    {
        return RecordTypes.Contains(type.Type.GetPure());
    }
}