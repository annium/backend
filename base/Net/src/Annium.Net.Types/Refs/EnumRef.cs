using Annium.Core.Runtime.Types;

namespace Annium.Net.Types.Refs;

[ResolutionKeyValue(RefType.Enum)]
public sealed record EnumRef(string Namespace, string Name) : IRef
{
    public RefType Type => RefType.Enum;
}