using System;
using System.Collections.Generic;
using Annium.Core.Primitives;
using Annium.Core.Reflection;
using Annium.Net.Types.Models;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Map;

internal static partial class Map
{
    private static readonly Type BaseRecordType = typeof(IEnumerable<>).MakeGenericType(typeof(KeyValuePair<,>));
    private static readonly HashSet<Type> RecordTypes = new();

    private static void RegisterRecords()
    {
        RegisterRecordType(typeof(IDictionary<,>));
        RegisterRecordType(typeof(IReadOnlyDictionary<,>));
        RegisterRecordType(typeof(Dictionary<,>));
    }

    private static void RegisterRecordType(Type type)
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

    private static ITypeModel? ToRecord(Type type)
    {
        if (!RecordTypes.Contains(type.IsGenericType ? type.GetGenericTypeDefinition() : type))
            return null;

        var args = type.GetTargetImplementation(BaseArrayType)!.ToContextualType().GenericArguments[0].GenericArguments;
        var keyTypeModel = ToModel(args[0]);
        var valueTypeModel = ToModel(args[1]);

        var model = new RecordModel(keyTypeModel, valueTypeModel);
        Console.WriteLine($"Mapped {type} -> {model}");

        return model;
    }
}