using System.Collections.Generic;
using Annium.Logging;
using Annium.Net.Types.Internal.Extensions;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Processors;

internal class Processor : ILogSubject
{
    public ILogger Logger { get; }
    private readonly IEnumerable<IProcessor> _processors;

    public Processor(
        IEnumerable<IProcessor> processors,
        ILogger logger
    )
    {
        Logger = logger;
        _processors = processors;
    }

    public void Process(ContextualType type, IProcessingContext ctx) => Process(type, type.Nullability, ctx);

    public void Process(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        this.Trace($"Process {type.FriendlyName()}");

        foreach (var processor in _processors)
        {
            var result = processor.Process(type, nullability, ctx);
            if (!result)
                continue;

            this.Trace($"Processed {type.FriendlyName()} via {processor.GetType().FriendlyName()}");
            return;
        }
    }
}