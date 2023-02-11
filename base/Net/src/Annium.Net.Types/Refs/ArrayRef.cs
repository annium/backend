using Annium.Core.Runtime.Types;

namespace Annium.Net.Types.Refs;

[ResolutionKeyValue(RefType.Array)]
public record ArrayRef(IRef Value) : IRef
{
    public RefType Type => RefType.Array;
}