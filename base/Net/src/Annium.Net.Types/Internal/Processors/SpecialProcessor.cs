using System;
using System.Threading.Tasks;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Processors;

internal static class SpecialProcessor
{
    public static bool Process(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        return type.Type.IsGenericType
            ? ProcessGeneric(type, type.Type.GetGenericTypeDefinition(), ctx)
            : ProcessNonGeneric(type);
    }

    private static bool ProcessGeneric(ContextualType type, Type definition, IProcessingContext ctx)
    {
        if (definition == typeof(Task<>) || definition == typeof(ValueTask<>))
        {
            var arg = type.GenericArguments[0];
            ctx.Process(arg);
            return true;
        }

        return false;
    }

    private static bool ProcessNonGeneric(ContextualType type)
    {
        if (type.Type == typeof(Task) || type.Type == typeof(ValueTask))
            return true;

        return false;
    }
}