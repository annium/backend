using Annium.Net.Types.Internal.Helpers;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Referrers;

internal class StructReferrer : IReferrer
{
    public IRef GetRef(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        var modelRef = this.BuildRef(type, ctx, static (ns, name, args) => new StructRef(ns, name, args));

        return modelRef;
    }
}