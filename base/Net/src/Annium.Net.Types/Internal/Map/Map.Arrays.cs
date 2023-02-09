using System;
using System.Collections.Generic;
using Annium.Core.Primitives;
using Annium.Core.Reflection;
using Annium.Net.Types.Models;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Map;

internal static partial class Map
{
    private static readonly Type BaseArrayType = typeof(IEnumerable<>);
    private static readonly HashSet<Type> ArrayTypes = new();

    private static void RegisterArrays()
    {
        RegisterArrayType(typeof(IEnumerable<>));
        RegisterArrayType(typeof(IReadOnlyCollection<>));
        RegisterArrayType(typeof(ICollection<>));
        RegisterArrayType(typeof(IReadOnlyList<>));
        RegisterArrayType(typeof(IList<>));
        RegisterArrayType(typeof(IReadOnlySet<>));
        RegisterArrayType(typeof(ISet<>));
        RegisterArrayType(typeof(List<>));
        RegisterArrayType(typeof(HashSet<>));
    }

    public static void RegisterArrayType(Type type)
    {
        if (type.IsGenericParameter)
            throw new ArgumentException($"Can't register generic parameter {type.FriendlyName()} as array type");

        if (type is { IsGenericType: true, IsGenericTypeDefinition: false })
            throw new ArgumentException($"Can't register generic type {type.FriendlyName()} as array type");

        if (type != BaseArrayType && !type.IsDerivedFrom(BaseArrayType))
            throw new ArgumentException($"Type {type.FriendlyName()} doesn't implement {BaseArrayType.FriendlyName()}");

        if (!ArrayTypes.Add(type))
            throw new ArgumentException($"Type {type.FriendlyName()} is already registered as array type");
    }

    private static ITypeModel? ToArray(ContextualType type)
    {
        if (!type.Type.IsArray || !ArrayTypes.Contains(type.Type.IsGenericType ? type.Type.GetGenericTypeDefinition() : type.Type))
            return null;

        var elementType = type.Type.IsArray
            ? type.Type.ToContextualType().GenericArguments[0] ?? throw new InvalidOperationException($"Failed to resolve element type of {type.Type.FriendlyName()}")
            : type.Type.GetTargetImplementation(BaseArrayType)!.ToContextualType().GenericArguments[0];
        var elementTypeModel = ToModel(elementType);
        var model = new ArrayModel(elementTypeModel);
        Console.WriteLine($"Mapped {type} -> {model}");

        return model;
    }
}