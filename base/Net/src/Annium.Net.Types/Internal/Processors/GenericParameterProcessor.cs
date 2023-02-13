using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Processors;

internal class GenericParameterProcessor : IProcessor
{
    public bool Process(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        return type.Type.IsGenericParameter;
    }
}