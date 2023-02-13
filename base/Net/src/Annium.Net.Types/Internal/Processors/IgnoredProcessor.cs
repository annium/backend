using Annium.Net.Types.Internal.Extensions;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Processors;

internal class IgnoredProcessor : IProcessor
{
    public bool Process(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        if (!MapperConfig.IsIgnored(type))
            return false;

        if (!type.Type.IsGenericType)
            return true;

        var typeGenericArguments = type.GetGenericArguments();
        foreach (var arg in typeGenericArguments)
            ctx.Process(arg);

        return true;
    }
}