using System;
using Annium.Core.Primitives;
using Annium.Core.Runtime.Types;

namespace Annium.Net.Types.Refs;

[ResolutionKeyValue(RefType.Struct)]
public sealed record StructRef(string Namespace, string Name, params IRef[] Args) : IRef
{
    public RefType Type => RefType.Struct;
    public override int GetHashCode() => HashCode.Combine(Namespace, Name, HashCodeSeq.Combine(Args));
    public bool Equals(StructRef? other) => GetHashCode() == other?.GetHashCode();
}