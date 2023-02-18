using System;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Core.Primitives;

public static class TypeRecordExtensions
{
    public static bool IsRecordLike(this Type type) =>
        type.TryGetRecordElementTypes() is not null;

    public static (Type keyType, Type valueType) GetRecordElementTypes(this Type type) =>
        type.TryGetRecordElementTypes() ?? throw new InvalidOperationException($"Type {type.FriendlyName()} is not array-like type");

    public static (Type keyType, Type valueType)? TryGetRecordElementTypes(this Type type)
    {
        var arrayImplementation = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
            ? type
            : type.GetInterfaces()
                .SingleOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        if (arrayImplementation is null)
            return null;

        var elementType = arrayImplementation.GetGenericArguments()[0];
        if (!elementType.IsGenericType || elementType.GetGenericTypeDefinition() != typeof(KeyValuePair<,>))
            return null;

        var elementTypeArguments = elementType.GetGenericArguments();

        return (elementTypeArguments[0], elementTypeArguments[1]);
    }
}