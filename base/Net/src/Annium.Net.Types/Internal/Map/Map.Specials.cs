using System;
using System.Threading.Tasks;
using Annium.Net.Types.Models;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Map;

internal static partial class Map
{
    private static ITypeModel? ToSpecial(ContextualType type)
    {
        return type.Type.IsGenericType
            ? ToGenericSpecial(type, type.Type.GetGenericTypeDefinition())
            : ToNonGenericSpecial(type);
    }

    private static ITypeModel? ToGenericSpecial(ContextualType type, Type definition)
    {
        if (definition == typeof(Task<>) || definition == typeof(ValueTask<>))
        {
            var arg = type.GenericArguments[0];
            return ToModel(arg);
        }

        return null;
    }

    private static ITypeModel? ToNonGenericSpecial(Type type)
    {
        if (type == typeof(Task) || type == typeof(ValueTask))
            return ToModel(typeof(void).ToContextualType(), false);

        return null;
    }
}