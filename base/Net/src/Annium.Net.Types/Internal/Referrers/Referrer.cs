using System;
using System.Collections.Generic;
using Annium.Debug;
using Annium.Internal;
using Annium.Net.Types.Internal.Extensions;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Referrers;

internal class Referrer
{
    private readonly IEnumerable<IReferrer> _referrers;

    public Referrer(IEnumerable<IReferrer> referrers)
    {
        _referrers = referrers;
    }

    public IRef GetRef(ContextualType type, IProcessingContext ctx) => GetRef(type, type.Nullability, ctx);

    public IRef GetRef(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        this.Trace($"Resolve {type.FriendlyName()} ref");

        foreach (var referrer in _referrers)
        {
            var result = referrer.GetRef(type, nullability, ctx);
            if (result is null)
                continue;

            this.Trace($"Resolved {type.FriendlyName()} ref as {result} via {referrer.GetType().FriendlyName()}");
            return result;
        }

        throw new InvalidOperationException($"Failed to resolve {type.FriendlyName()} ref");
    }
}