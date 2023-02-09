using System;
using System.Threading.Tasks;
using Annium.Net.Types.Models;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Mappers;

internal static class SpecialMapper
{
    public static ITypeModel? Map(ContextualType type, IMapperContext ctx)
    {
        return type.Type.IsGenericType
            ? MapGeneric(type, type.Type.GetGenericTypeDefinition(), ctx)
            : MapNonGeneric(type, ctx);
    }

    private static ITypeModel? MapGeneric(ContextualType type, Type definition, IMapperContext ctx)
    {
        if (definition == typeof(Task<>) || definition == typeof(ValueTask<>))
        {
            var arg = type.GenericArguments[0];
            return ctx.Map(arg);
        }

        return null;
    }

    private static ITypeModel? MapNonGeneric(ContextualType type, IMapperContext ctx)
    {
        if (type.Type == typeof(Task) || type.Type == typeof(ValueTask))
            return ctx.Map(typeof(void).ToContextualType());

        return null;
    }
}