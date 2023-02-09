using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Processors;

internal static class GenericParameterProcessor
{
    public static bool Process(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        return type.Type.IsGenericParameter;
    }
}