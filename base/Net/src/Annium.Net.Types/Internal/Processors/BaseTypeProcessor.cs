using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Processors;

internal class BaseTypeProcessor : IProcessor
{
    public bool Process(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        return ctx.Config.GetBaseTypeRefFor(type.Type) is not null;
    }
}