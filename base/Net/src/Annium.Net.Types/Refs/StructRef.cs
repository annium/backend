using Annium.Core.Runtime.Types;

namespace Annium.Net.Types.Refs;

[ResolutionKeyValue(RefType.Struct)]
public sealed record StructRef(string Namespace, string Name, params IRef[] Args) : IRef
{
    public RefType Type => RefType.Struct;
}