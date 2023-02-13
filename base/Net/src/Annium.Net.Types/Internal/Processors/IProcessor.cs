using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Processors;

internal interface IProcessor
{
    // int Order { get; }
    bool Process(ContextualType type, Nullability nullability, IProcessingContext ctx);
}