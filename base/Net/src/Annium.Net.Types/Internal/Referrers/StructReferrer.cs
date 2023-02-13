using System.Linq;
using Annium.Net.Types.Internal.Extensions;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Referrers;

internal class StructReferrer : IReferrer
{
    public IRef GetRef(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        var pure = type.Type.GetPure().ToContextualType();
        var baseRef = (StructRef) ctx.RequireRef(pure);
        var name = type.FriendlyName();
        if (type.Type.IsGenericType)
            name = name[..name.IndexOf('<')];

        var typeGenericArguments = type.GetGenericArguments();
        var genericArguments = typeGenericArguments.Select(ctx.GetRef).ToArray();
        var modelRef = new StructRef(baseRef.Namespace, name, genericArguments);

        return modelRef;
    }
}