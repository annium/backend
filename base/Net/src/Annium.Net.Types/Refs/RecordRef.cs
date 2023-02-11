using Annium.Core.Runtime.Types;

namespace Annium.Net.Types.Refs;

[ResolutionKeyValue(RefType.Record)]
public record RecordRef(IRef Key, IRef Value) : IRef
{
    public RefType Type => RefType.Record;
}