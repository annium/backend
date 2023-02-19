using Annium.Net.Types.Internal.Helpers;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Referrers;

internal class KnownReferrer : IReferrer
{
    private static StructRef BuildStructRef(string ns, string name, IRef[] args) => new(ns, name, args);
    private static InterfaceRef BuildInterfaceRef(string ns, string name, IRef[] args) => new(ns, name, args);
    private static EnumRef BuildEnumRef(string ns, string name, IRef[] args) => new(ns, name);

    public IRef? GetRef(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        if (!ctx.Config.IsKnown(type))
            return null;

        if (type.Type.IsEnum)
            return this.BuildRef(type, ctx, BuildEnumRef);

        if (type.Type.IsInterface)
            return this.BuildRef(type, ctx, BuildInterfaceRef);

        return this.BuildRef(type, ctx, BuildStructRef);
    }
}