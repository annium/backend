using System.Collections.Generic;
using Annium.Net.Types.Refs;

namespace Annium.Net.Types.Models;

public sealed record StructModel(
    Namespace Namespace,
    string Name,
    IReadOnlyList<IRef> Args,
    IRef? Base,
    IReadOnlyList<IRef> Interfaces,
    IReadOnlyList<FieldModel> Fields
) : ModelBase(Namespace, Name)
{
    public override string ToString() => $"struct {Namespace}.{Name}";
}