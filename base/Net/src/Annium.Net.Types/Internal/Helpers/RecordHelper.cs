using System;
using System.Linq;
using Annium.Net.Types.Internal.Extensions;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Helpers;

internal static class RecordHelper
{
    public static (ContextualType keyType, ContextualType valueType) ResolveElementType(ContextualType type)
    {
        var arrayImplementation = type.GetInterfaces()
            .SingleOrDefault(x => x.Type.IsGenericType && x.Type.GetGenericTypeDefinition() == MapperConfig.BaseArrayType);
        if (arrayImplementation is null || arrayImplementation.GenericArguments[0].Type.GetGenericTypeDefinition() != MapperConfig.BaseRecordValueType)
            throw new InvalidOperationException($"Failed to resolve key/value types of {type.FriendlyName()}");
        var keyType = arrayImplementation.GenericArguments[0].GenericArguments[0];
        var valueType = arrayImplementation.GenericArguments[0].GenericArguments[1];

        return (keyType, valueType);
    }
}