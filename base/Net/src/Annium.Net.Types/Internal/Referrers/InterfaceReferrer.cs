using System.Linq;
using Annium.Net.Types.Internal.Extensions;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Referrers;

internal class InterfaceReferrer : IReferrer
{
    public IRef? GetRef(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        if (!type.Type.IsInterface)
            return null;

        var pure = type.Type.GetPure().ToContextualType();
        var baseRef = (InterfaceRef) ctx.RequireRef(pure);
        var name = type.FriendlyName();
        if (type.Type.IsGenericType)
            name = name[..name.IndexOf('<')];

        var typeGenericArguments = type.GetGenericArguments();
        var genericArguments = typeGenericArguments.Select(ctx.GetRef).ToArray();
        var modelRef = new InterfaceRef(baseRef.Namespace, name, genericArguments);

        return modelRef;
    }
}