using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Processors;

internal static class BaseTypeProcessor
{
    public static bool Process(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        return BaseType.GetRefFor(type.Type) is not null;
    }
}