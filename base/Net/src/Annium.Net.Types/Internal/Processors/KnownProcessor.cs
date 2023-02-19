using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Processors;

internal class KnownProcessor : IProcessor
{
    public bool Process(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        return ctx.Config.IsKnown(type);
    }
}