using System.Collections.Generic;

namespace Annium.Net.Types.Models;

public sealed record EnumModel(
    Namespace Namespace,
    string Name,
    IReadOnlyDictionary<string, long> Values
) : TypeModelBase(Namespace, Name)
{
    public override string ToString() => $"enum {Namespace}.{Name}";
}