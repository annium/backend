using System;
using System.Collections.Generic;
using Annium.Core.Internal;
using Annium.Core.Primitives;
using Annium.Net.Types.Models;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Referrers;

internal static class Referrer
{
    private delegate ModelRef? Handler(ContextualType type, Nullability nullability, IProcessingContext ctx);

    private static readonly IReadOnlyList<Handler> Handlers;

    static Referrer()
    {
        var handlers = new List<Handler>
        {
            NullableReferrer.GetRef,
            GenericParameterReferrer.GetRef,
            BaseTypeReferrer.GetRef,
            EnumReferrer.GetRef,
        };

        Handlers = handlers;
    }

    public static ModelRef GetRef(ContextualType type, IProcessingContext ctx) => GetRef(type, type.Nullability, ctx);

    public static ModelRef GetRef(ContextualType type, Nullability nullability, IProcessingContext ctx)
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