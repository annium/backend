using System.Collections.Generic;
using System.Linq;
using Annium.Core.Primitives.Collections.Generic;

namespace Annium.Net.Types.Models;

public sealed record StructModel(
    Namespace Namespace,
    string Name,
    IReadOnlyList<ModelRef> GenericArguments,
    ModelRef? Base,
    IReadOnlyList<ModelRef> Interfaces,
    IReadOnlyList<FieldModel> Fields
) : TypeModelBase(Namespace, ResolveName(Name, GenericArguments))
{
    private static string ResolveName(string name, IReadOnlyCollection<ModelRef> genericArguments) =>
        genericArguments.Count == 0
            ? name
            : $"{name}<{genericArguments.Select(x => x.Name).Join(", ")}>";

    public override string ToString() => $"struct {Namespace}.{Name}";
}