using Annium.Core.Runtime.Types;

namespace Annium.Net.Types.Refs;

[ResolutionKeyValue(RefType.Nullable)]
public record NullableRef(IRef Value) : IRef
{
    public RefType Type => RefType.Nullable;
}