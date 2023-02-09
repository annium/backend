using Annium.Net.Types.Models;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Processors;

internal static class BaseTypeProcessor
{
    public static bool Process(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        return BaseType.GetFor(type.Type) is not null;
    }
}