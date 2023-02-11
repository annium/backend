using System;
using System.Linq;
using Annium.Net.Types.Internal.Extensions;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Helpers;

internal static class ArrayHelper
{
    public static ContextualType ResolveElementType(ContextualType type)
    {
        ContextualType? elementType;
        if (type.Type.IsArray)
            elementType = type.Type.GetElementType()?.ToContextualType();
        else
        {
            var arrayImplementation = type.GetInterfaces()
                .SingleOrDefault(x => x.Type.IsGenericType && x.Type.GetGenericTypeDefinition() == MapperConfig.BaseArrayType);
            elementType = arrayImplementation?.GenericArguments[0];
        }

        return elementType ?? throw new InvalidOperationException($"Failed to resolve element type of {type.FriendlyName()}");
    }
}