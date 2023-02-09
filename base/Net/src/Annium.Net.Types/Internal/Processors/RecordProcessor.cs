using System;
using Annium.Core.Primitives;
using Annium.Core.Reflection;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Processors;

internal static class RecordProcessor
{
    public static bool Process(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        if (!MapperConfig.IsRecord(type))
            return false;

        var args = type.Type.GetTargetImplementation(MapperConfig.BaseRecordType)?.ToContextualType().GenericArguments[0].GenericArguments
            ?? throw new InvalidOperationException($"Failed to resolve key/value types of {type.Type.FriendlyName()}");

        ctx.Process(args[0]);
        ctx.Process(args[1]);

        return true;
    }
}