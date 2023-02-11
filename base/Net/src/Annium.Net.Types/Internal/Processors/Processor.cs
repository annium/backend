using System.Collections.Generic;
using Annium.Core.Internal;
using Annium.Core.Primitives;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Processors;

internal static class Processor
{
    private delegate bool Handler(ContextualType type, Nullability nullability, IProcessingContext ctx);

    private static readonly IReadOnlyList<Handler> Handlers;

    static Processor()
    {
        var handlers = new List<Handler>
        {
            NullableProcessor.Process,
            GenericParameterProcessor.Process,
            BaseTypeProcessor.Process,
            EnumProcessor.Process,
            SpecialProcessor.Process,
            RecordProcessor.Process,
            ArrayProcessor.Process,
            StructProcessor.Process,
        };

        Handlers = handlers;
    }

    public static void Process(ContextualType type, IProcessingContext ctx) => Process(type, type.Nullability, ctx);

    public static void Process(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        Log.Trace($"Process {type}");

        foreach (var handler in Handlers)
        {
            var result = handler(type, nullability, ctx);
            if (!result)
                continue;

            Log.Trace($"Processed {type} via {handler.Method.DeclaringType!.FriendlyName()}");
            return;
        }
    }
}