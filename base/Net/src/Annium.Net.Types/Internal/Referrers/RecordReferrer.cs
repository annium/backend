using System;
using Annium.Core.Reflection;
using Annium.Net.Types.Internal.Extensions;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Referrers;

internal static class RecordReferrer
{
    public static IRef? GetRef(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        if (!MapperConfig.IsRecord(type))
            return null;

        var args = type.Type.GetTargetImplementation(MapperConfig.BaseArrayType)?.ToContextualType().GenericArguments[0].GenericArguments
            ?? throw new InvalidOperationException($"Failed to resolve key/value types of {type.FriendlyName()}");

        var keyRef = ctx.GetRef(args[0]);
        var valueRef = ctx.GetRef(args[1]);

        return new RecordRef(keyRef, valueRef);
    }
}