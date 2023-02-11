using System;
using System.Collections.Generic;
using Annium.Core.Internal;
using Annium.Core.Primitives;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Referrers;

internal static class Referrer
{
    private delegate IRef? Handler(ContextualType type, Nullability nullability, IProcessingContext ctx);

    private static readonly IReadOnlyList<Handler> Handlers;

    static Referrer()
    {
        var handlers = new List<Handler>
        {
            NullableReferrer.GetRef,
            GenericParameterReferrer.GetRef,
            BaseTypeReferrer.GetRef,
            EnumReferrer.GetRef,
            SpecialReferrer.GetRef,
            RecordReferrer.GetRef,
            ArrayReferrer.GetRef,
            StructReferrer.GetRef,
        };

        Handlers = handlers;
    }

    public static IRef GetRef(ContextualType type, IProcessingContext ctx) => GetRef(type, type.Nullability, ctx);

    public static IRef GetRef(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        Log.Trace($"Resolve {type} ref");

        foreach (var handler in Handlers)
        {
            var result = handler(type, nullability, ctx);
            if (result is null)
                continue;

            Log.Trace($"Resolved {type} ref as {result} via {handler.Method.DeclaringType!.FriendlyName()}");
            return result;
        }

        throw new InvalidOperationException($"Failed to resolve {type} ref");
    }
}