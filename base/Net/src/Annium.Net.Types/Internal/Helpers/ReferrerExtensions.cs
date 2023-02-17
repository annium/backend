using System;
using System.Linq;
using Annium.Core.Internal;
using Annium.Net.Types.Internal.Extensions;
using Annium.Net.Types.Internal.Referrers;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Helpers;

internal static class ReferrerExtensions
{
    public static TRef BuildRef<TRef>(this IReferrer referrer, ContextualType type, IProcessingContext ctx, Func<string, string, IRef[], TRef> factory)
        where TRef : IModelRef
    {
        var pure = type.Type.GetPure().ToContextualType();
        var baseRef = (TRef) ctx.RequireRef(pure);
        var name = type.PureName();
        var typeGenericArguments = type.GetGenericArguments();
        var genericArguments = typeGenericArguments.Select(ctx.GetRef).ToArray();
        var modelRef = factory(baseRef.Namespace, name, genericArguments);
        referrer.Trace($"Created {type.FriendlyName()} ref as {modelRef}");

        return modelRef;
    }
}